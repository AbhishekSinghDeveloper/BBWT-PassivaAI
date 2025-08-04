using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Web.Extensions;
using BBWM.ReCaptcha;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;
using System.Text;

namespace BBWM.Core.Membership.Services;

/// <summary>
/// Security service implementation.
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly IDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ISettingsService _settingsService;
    private readonly ILoginAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SecurityService(
        IDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        ISettingsService settingsService,
        ILoginAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _settingsService = settingsService;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LockedOutIp> GetLongestActiveLockingByIp(string ip, CancellationToken cancellationToken = default) =>
        await _context.Set<LockedOutIp>()
            .Where(x => x.IpAddress == ip && x.LockoutEnd > DateTime.Now)
            .OrderByDescending(x => x.LockoutEnd)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task CheckIpLockOut(string ip, CancellationToken cancellationToken = default)
    {
        var settings = _settingsService.GetSettingsSection<FailedAttemptsPasswordSettings>();
        if (settings is null)
            throw new ConflictException("Settings for failed login attempts not found.");

        if (settings.LockTypeAccount == LockType.NeverLock ||
            await _auditService.GetLastAttemptsCountAsync(ip, DateTimeOffset.Now.AddMinutes(-settings.PasswordAttemptWindow.GetValueOrDefault())) <
            settings.MaxInvalidPasswordAttempts) return;

        await LockOutByIp(ip, settings.IntervalInSeconds.GetValueOrDefault(), cancellationToken);
        await _auditService.SaveLoginAuditAsync(null, "Blocked by IP");
    }

    public async Task<bool> TryLockUserOnInvalidRecaptcha(User user, bool isReCaptchaValid, CancellationToken cancellationToken = default)
    {
        var reCaptchaSettings = _settingsService.GetSettingsSection<ReCaptchaSettings>();

        if (reCaptchaSettings.ValidateOnLoginEnabled is null || !(bool)reCaptchaSettings.ValidateOnLoginEnabled) return false;

        var lockIntervalInSeconds = reCaptchaSettings.LockIntervalInSeconds.GetValueOrDefault();
        var isUserLockRequired = !isReCaptchaValid && !user.U2fEnabled && !user.TwoFactorEnabled;

        if (!isUserLockRequired) return false;

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddSeconds(lockIntervalInSeconds));
        await _auditService.SaveLoginAuditAsync(user, "Blocked because of failed recaptcha");
        return true;
    }

    public async Task<bool> IsIpLocked(string ip, CancellationToken cancellationToken = default) =>
        await GetLongestActiveLockingByIp(ip, cancellationToken) is not null;

    public async Task AddFailedAttemptForUser(string userId, CancellationToken cancellationToken = default) =>
        await AddFailedAttemptForUser(await _userManager.FindByIdAsync(userId), cancellationToken);

    public async Task AddFailedAttemptForUser(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new UserNotExistsException();

        var settings = _settingsService.GetSettingsSection<FailedAttemptsPasswordSettings>();
        if (settings is null)
            throw new ConflictException("Settings for failed login attempts not found.");

        if (user.LockoutEnabled)
            throw new ConflictException("User is already locked.");

        if (settings.LockTypeAccount == LockType.NeverLock) return;

        if (user.FirstPasswordFailureDate is null ||
            (DateTimeOffset.Now - (DateTimeOffset)user.FirstPasswordFailureDate).Minutes > settings.PasswordAttemptWindow)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            user.FirstPasswordFailureDate = DateTimeOffset.Now;
            await _userManager.UpdateAsync(user);
        }

        await _userManager.AccessFailedAsync(user);

        if (user.AccessFailedCount >= settings.MaxInvalidPasswordAttempts)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.ResetAccessFailedCountAsync(user);
            if (settings.UnlockTypeAccount == UnlockType.Temporary)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddSeconds(settings.IntervalInSeconds.GetValueOrDefault()));
                await _auditService.SaveLoginAuditAsync(user, "Blocked because of exceeded attempts to input an incorrect username / password");
            }
        };

        _context.Set<UserPasswordFailedHistory>().Add(new UserPasswordFailedHistory
        {
            email = user.Email,
            failedDate = DateTime.Now,
            IpAddress = _httpContextAccessor.HttpContext.GetUserIp()
        });
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockUser(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new UserNotExistsException();

        await _userManager.SetLockoutEnabledAsync(user, false);
        user.FirstPasswordFailureDate = null;
        await _userManager.UpdateAsync(user);
    }


    public string CheckUsersNewPassword(User user, string newPassword)
    {
        if (user is null) throw new UserNotExistsException();

        var settings = _settingsService.GetSettingsSection<UserPasswordSettings>();
        if (settings is null)
            throw new ConflictException("Settings for passwords validation not found.");

        if (string.IsNullOrEmpty(newPassword))
            return SecurityErrorMessages.PasswordCannotBeEmpty;

        if (newPassword == GetHashedPassword(user.Email))
            return SecurityErrorMessages.PasswordDifferentFromEmail;

        switch (settings.PasswordReuse)
        {
            case PasswordReuseSettings.NeverUse:
                {
                    var passwordHistoryObj = FindPasswordInPasswordHistory(user, newPassword, settings);
                    if (passwordHistoryObj is not null)
                        return SecurityErrorMessages.PasswordCannotBeReUsed;
                }
                break;

            case PasswordReuseSettings.MayReUse:
                {
                    var passwordHistoryObj = FindPasswordInPasswordHistory(user, newPassword, settings);
                    if (passwordHistoryObj is not null)
                        return SecurityErrorMessages.PasswordMayReUse(settings.LastPasswordsNumber.GetValueOrDefault());
                }
                break;

            case PasswordReuseSettings.MayUse:
            default:
                break;
        }

        return null;
    }

    public async Task SavePasswordToHistory(User user, CancellationToken cancellationToken = default)
    {
        if (user is null) throw new UserNotExistsException();

        _context.Set<PasswordHistory>().Add(new PasswordHistory
        {
            UserId = user.Id,
            Password = user.PasswordHash,
            CreateDate = DateTimeOffset.Now,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public string GetHashedPassword(string value)
    {
        var sb = new StringBuilder();

        using (var shaM = SHA512.Create())
        {
            var data = Encoding.UTF8.GetBytes(value);
            var dataHash = shaM.ComputeHash(data);

            // Convert to string
            foreach (var t in dataHash)
            {
                sb.Append(t.ToString("X2"));
            }
        }

        return sb.ToString().ToLowerInvariant();
    }

    public static class SecurityErrorMessages
    {
        public static readonly string PasswordDifferentFromEmail = "Your new password must be different from your email.";
        public static readonly string PasswordCannotBeReUsed = "The previously used password cannot be set. Please create a new password.";
        public static string PasswordMayReUse(int n) => $"Your new password must be different from your last {n} records in password history.";
        public static string PasswordCannotBeEmpty = "Password can not be empty.";
    }

    private async Task LockOutByIp(string ip, int intervalInSeconds, CancellationToken cancellationToken)
    {
        if (intervalInSeconds > 0)
        {
            await _context.Set<LockedOutIp>().AddAsync(
                new LockedOutIp
                {
                    IpAddress = ip,
                    LockoutEnd = DateTime.Now.AddSeconds(intervalInSeconds)
                },
                cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private PasswordHistory FindPasswordInPasswordHistory(User user, string enteredPassword, UserPasswordSettings reuseSettings)
    {
        var passwordHasher = new PasswordHasher<User>();
        var passwordsHistory = _context.Set<PasswordHistory>().Where(p => p.UserId == user.Id).ToList();

        if (reuseSettings.PasswordReuse == PasswordReuseSettings.MayReUse)
        {
            // Users may use any password that they haven’t used in the last N passwords
            passwordsHistory = passwordsHistory.OrderByDescending(p => p.CreateDate).Take(reuseSettings.LastPasswordsNumber.GetValueOrDefault()).ToList();
        }

        foreach (var passwordHistoryObj in passwordsHistory)
        {
            if (passwordHasher.VerifyHashedPassword(user, passwordHistoryObj.Password, enteredPassword) ==
                PasswordVerificationResult.Success)
            {
                return passwordHistoryObj;
            }
        }

        return null;
    }
}