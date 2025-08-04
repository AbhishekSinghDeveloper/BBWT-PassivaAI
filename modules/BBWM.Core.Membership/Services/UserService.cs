using AutoMapper;
using BBWM.Core.AppEnvironment;
using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Extensions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.Services;
using BBWM.Core.Web.Extensions;
using BBWM.Messages;
using BBWM.Messages.Templates;
using BBWM.ReCaptcha;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Specialized;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using BBWM.Core.Extensions;
using U2F.Core.Models;
using U2F.Core.Utils;

using ClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BBWM.Core.Membership.Services;

public class UserService : IUserService
{
    private readonly IDataService _dataService;
    private readonly IUserDataService _userDataService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly UserLoginSettings _loginSettings;
    private readonly ISecurityService _securityService;
    private readonly IAllowedIpService _allowedIpService;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPwnedPasswordProvider _pwnedPasswordProvider;
    private readonly ISettingsService _settingsService;
    private readonly IReCaptchaService _recaptchaService;
    private readonly ILoginAuditService _loginAuditService;
    private readonly IMapper _mapper;
    private readonly IDbContext _context;
    private readonly IAppEnvironmentService _appEnvironmentService;
    private readonly IUser2FAService _user2FAService;

    public UserService(
        IDataService dataService,
        IUserDataService userDataService,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptionsSnapshot<UserLoginSettings> loginSettingsOptions,
        ISecurityService securityService,
        IAllowedIpService allowedIpService,
        IEmailSender emailSender,
        IEmailTemplateService emailTemplateService,
        IHttpContextAccessor httpContextAccessor,
        IPwnedPasswordProvider pwnedPasswordPrivider,
        ISettingsService settingsService,
        IReCaptchaService recaptchaService,
        ILoginAuditService loginAuditService,
        IDbContext context,
        IAppEnvironmentService appEnvironmentService,
        IUser2FAService user2FAService,
        IMapper mapper)
    {
        _context = context;
        _appEnvironmentService = appEnvironmentService;
        _user2FAService = user2FAService;
        _mapper = mapper;
        _dataService = dataService;
        _userDataService = userDataService;
        _userManager = userManager;
        _signInManager = signInManager;
        _securityService = securityService;
        _allowedIpService = allowedIpService;
        _loginSettings = loginSettingsOptions?.Value;
        _emailSender = emailSender;
        _emailTemplateService = emailTemplateService;
        _httpContextAccessor = httpContextAccessor;
        _pwnedPasswordProvider = pwnedPasswordPrivider;
        _settingsService = settingsService;
        _recaptchaService = recaptchaService;
        _loginAuditService = loginAuditService;
    }

    private string DomainUrl => _httpContextAccessor.HttpContext.GetDomainUrl();

    public async Task CheckRecoveryCodeExists(RecoveryCodeDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.PasswordResetToken)
            .FirstOrDefaultAsync(x => x.Id == dto.UserId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        if (user.PasswordResetToken is null)
            throw new BusinessException(ErrorMessages.RecoveryNotFoundForUser);

        var passwordResetToken = await _context.Set<ActivationToken>()
            .FirstOrDefaultAsync(o => o.Token == dto.Code, cancellationToken);
        if (passwordResetToken is null) throw new ObjectNotExistsException("Recovery code not found.");

        if (passwordResetToken.ExpirationDate is not null && passwordResetToken.ExpirationDate <= DateTime.Now)
            throw new BusinessException(ErrorMessages.RecoveryExpired);

        if (user.PasswordResetToken.Token != dto.Code)
            throw new BusinessException(ErrorMessages.RecoveryInvalid);
    }

    public async Task<AccountActivationInfoDTO> GetAccountActivationInfo(string userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.InvitationToken)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus != AccountStatus.Invited)
            throw new BusinessException(ActivationError.ActivationCompleted.ToEnumValueString());

        if (user.InvitationToken is null)
            throw new BusinessException(ActivationError.InvitationNotFoundForUser.ToEnumValueString());

        if (user.InvitationToken.Token != code)
            throw new BusinessException(ActivationError.ActivationCodeInvalid.ToEnumValueString());

        return new AccountActivationInfoDTO
        {
            UserId = user.Id,
            Email = user.Email,
            IsInvited = user.AccountStatus == AccountStatus.Invited
        };
    }

    public async Task<TwoFactorInfoDTO> GetTwoFactorInfo(TwoFactorInfoRequestDTO request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.PasswordResetToken)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        var isRequireUserTwoFactorAuthenticationForSettings = await RequireUserTwoFactorAuthenticationForSettings(user);

        return new TwoFactorInfoDTO
        {
            UserId = user.Id,
            U2fEnabled = user.U2fEnabled,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsRequireUserTwoFactorAuthenticationForSettings = isRequireUserTwoFactorAuthenticationForSettings
        };
    }

    public async Task<bool> RequireUserTwoFactorAuthenticationForSettings(User user)
    {
        // Check that user did set up 2FA in profile
        if (!user.U2fEnabled && !user.TwoFactorEnabled) return false;
        // 2FA ignore interval check
        var lastPassedTwoFactorCodeAuditAsync = await _loginAuditService.GetLastPassed2FACodeAuditAsync(user.Email);
        var twoFactorSettings = _settingsService.GetSettingsSection<TwoFactorSettings>();
        var twoFactorForSettingsIgnoreInterval = new TimeSpan(0, twoFactorSettings.AuthDurationMinutesForSettings.GetValueOrDefault(), 0);
        var intervalSinceLastPassedTwoFactorCode = DateTimeOffset.UtcNow - lastPassedTwoFactorCodeAuditAsync.Datetime;
        var is2FaIgnoreIntervalForSettingsExpired = intervalSinceLastPassedTwoFactorCode > twoFactorForSettingsIgnoreInterval;

        return is2FaIgnoreIntervalForSettingsExpired;
    }

    [IgnoreLogging(true)]
    public async Task<bool> CanImpersonate(string impersonatingUserId, string impersonatedUserId, CancellationToken cancellationToken = default)
    {
        var baseQuery = _userManager.Users.Include(o => o.UserRoles).ThenInclude(o => o.Role);

        var impersonatingUser = await baseQuery.FirstOrDefaultAsync(x => x.Id == impersonatingUserId, cancellationToken);
        if (impersonatingUser is null) throw new UserNotExistsException(ErrorMessages.ImpersonatingUserNotFound);

        var impersonatedUser = await baseQuery.FirstOrDefaultAsync(x => x.Id == impersonatedUserId, cancellationToken);
        if (impersonatedUser is null) throw new UserNotExistsException(ErrorMessages.ImpersonatedUserNotFound);

        return CanImpersonate(impersonatingUser, impersonatedUser);
    }

    [IgnoreLogging(true)]
    public async Task<ImpersonateUserDTO> IsUserImpersonating(ClaimsPrincipal userClaims)
    {
        var loggedUser = await _userDataService.Get(
            userClaims.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));
        var isImpersonating = userClaims.HasClaim(ClaimTypes.Impersonation.IsImpersonating, bool.TrueString);
        var claim = userClaims.FindFirst(ClaimTypes.Impersonation.OriginalUserName);
        var originalUserName = isImpersonating ? claim?.Value : string.Empty;

        return new ImpersonateUserDTO
        {
            ImpersonatedUserId = loggedUser is not null ? loggedUser.Id : string.Empty,
            ImpersonatedUserName = loggedUser is not null ? loggedUser.FullName : string.Empty,
            ImpersonatedUserEmail = loggedUser is not null ? loggedUser.Email : string.Empty,
            OriginalUserName = originalUserName,
            IsImpersonating = isImpersonating
        };
    }

    private static bool CanImpersonate(User impersonatingUser, User impersonatedUser) =>
        impersonatingUser.UserRoles.Any(a => a.Role.Name == Roles.SystemAdminRole) &&
        impersonatedUser.UserRoles.All(
            roleItem => roleItem.Role.Name != Roles.SystemAdminRole
            || roleItem.Role.Name != Roles.SuperAdminRole) &&
        !string.Equals(impersonatedUser.Email, impersonatingUser.Email, StringComparison.InvariantCultureIgnoreCase);

    public async Task<UserDTO> Invite(UserDTO dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new BusinessException(existingUser.AccountStatus == AccountStatus.Deleted
                ? ErrorMessages.EmailExistForDeleted
                : ErrorMessages.EmailExist);

        dto.UserName = dto.Email;
        dto.AccountStatus = AccountStatus.Invited;
        var savingResult = await _userDataService.Create(dto, cancellationToken);

        var createdUser = await _userManager.FindByIdAsync(savingResult.Id);
        await _userManager.UpdateAsync(createdUser);
        await SaveInvitationToken(
            createdUser,
            new ActivationToken
            {
                Token = await _userManager.GenerateUserInviteTokenAsync(createdUser),
                ExpirationDate = DateTime.Now.AddDays(InvitationExpireDays)
            },
            cancellationToken);

        await SendInvitation(createdUser, createdUser.InvitationToken.Token, cancellationToken);

        return await _userDataService.Get(createdUser.Id, cancellationToken);
    }

    public async Task ResendInvitation(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.InvitationToken)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus != AccountStatus.Invited)
            throw new BusinessException("An invitation can be re-sent only for invited users who didn't approve their invitations yet.");

        if (user.InvitationToken is null)
            throw new ConflictException("User doesn't have an invitation yet.");

        await SaveInvitationToken(
            user,
            new ActivationToken
            {
                Token = await _userManager.GenerateUserInviteTokenAsync(user),
                ExpirationDate = DateTime.Now.AddDays(InvitationExpireDays)
            },
            cancellationToken);

        await SendInvitation(user, user.InvitationToken.Token, cancellationToken);
    }

    public async Task<SignUpResultDTO> Register(UserRegistrationDTO dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new BusinessException($"User with email {dto.Email} already exists.");

        var registrationSettings = _settingsService.GetSettingsSection<RegistrationSettings>();

        if (registrationSettings is null)
            throw new ConflictException("Settings for the registration not found.");

        dto.OrganizationId = registrationSettings.SelfRegisterUserOrganizationId;

        var user = new User
        {
            UserName = dto.Email,
            FirstName = dto.FirstName?.Trim(),
            LastName = dto.LastName?.Trim(),
            Email = dto.Email
        };

        if (dto.OrganizationId is not null)
        {
            user.UserOrganizations.Add(new UserOrganization { OrganizationId = dto.OrganizationId.Value });
        }

        var signUpResult = new SignUpResultDTO
        {
            PwnedResult = await CheckPwnedPassword(dto)
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BusinessException(result.Errors.First().Description);

        if (AdminApprovalRequired())
        {
            user.AccountStatus = AccountStatus.Unapproved;
            await _userManager.UpdateAsync(user);
            signUpResult.AdminApprovalRequired = true;
        }
        else
        {
            user.AccountStatus = AccountStatus.Unverified;
            await _userManager.UpdateAsync(user);
            await SaveEmailConfirmationToken(
                user,
                new ActivationToken
                {
                    Token = await GenerateEmailConfirmationToken(user),
                    ExpirationDate = DateTime.Now.AddDays(EmailConfirmationExpireInDays)
                },
                cancellationToken);
            await SendEmailConfirmation(user, user.EmailConfirmationToken.Token, cancellationToken);
            signUpResult.ConfirmationSent = true;
        }

        await _securityService.SavePasswordToHistory(user, cancellationToken);

        return signUpResult;
    }

    public async Task<int> CheckPwnedPassword(UserRegistrationDTO userRegistrationData)
    {
        var nbr = 0;
        var search = userRegistrationData.PasswordSHA1.Substring(5);

        var res = await _pwnedPasswordProvider.GetPasswordPwned(userRegistrationData.PasswordSHA1);
        if (string.IsNullOrEmpty(res)) return nbr;

        var result = res.Split("\r\n").ToList();
        foreach (var str in result)
        {
            var tab = str.Split(':');
            if (string.Equals(tab[0], search, StringComparison.CurrentCultureIgnoreCase))
            {
                nbr = Convert.ToInt32(tab[1]);
            }
        }

        return nbr;
    }

    public async Task ResetPassword(ResetPasswordDTO dto, CancellationToken cancellationToken, bool verify = true)
    {
        if (string.IsNullOrEmpty(dto.Email))
            throw new ObjectNotExistsException(ErrorMessages.UndefinedEmail);

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken);
        if (user is null) throw new UserNotExistsException($"User with email {dto.Email} doesn't exist.");

        #region Seeded user case
        var isSeededUserRequiredPasswordRenewal = IsSeededUserRequiredPasswordRenewal(user);
        if (isSeededUserRequiredPasswordRenewal)
        {
            // Validate if new password matches one of the hardcoded accounts passwords.
            if (InitialUsers.NotAllowedAsNewUserPassword().Any(x => dto.Password == _securityService.GetHashedPassword(x)))
                throw new BusinessException("\"Your new password must be different from known passwords of seeded accounts.");
        }

        // This check considers a special case - when it's a first ever login to the system, we ignore showing email sending errors
        // to the seeded user as the system may have email settings not configured yet, whereas the seeded admin user should simply
        // reset the password without involving emails and log in.
        // This flag determined before resetting the password below
        var throwExceptionOnEmailSendingFailure = !isSeededUserRequiredPasswordRenewal;
        #endregion

        var validationMessage = _securityService.CheckUsersNewPassword(user, dto.Password);
        if (!string.IsNullOrEmpty(validationMessage)) throw new BusinessException(validationMessage);

        if (verify)
        {
            await CheckRecoveryCodeExists(new RecoveryCodeDTO { UserId = user.Id, Code = dto.Code }, cancellationToken);

            if (user.AccountStatus != AccountStatus.Active)
                throw new BusinessException("User is not activated.");

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Code));

            var result = await _userManager.ResetPasswordAsync(user, code, dto.Password);
            if (!result.Succeeded)
                throw new BusinessException(
                    string.Join(Environment.NewLine, result.Errors.Select(x => x.Description).ToList())
                );

            await RemovePasswordResetToken(user, cancellationToken);
        }

        if (user.LockoutEnabled)
            await _securityService.UnlockUser(user, cancellationToken);
        await _securityService.SavePasswordToHistory(user, cancellationToken);

        try
        {
            await SendPasswordChangedNotification(dto.Email, cancellationToken);
        }
        catch
        {
            if (throwExceptionOnEmailSendingFailure)
            {
                throw;
            }
        }
    }

    public async Task<AuthResultDTO> Login(LoginDTO dto, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .Include(o => o.UserRoles).ThenInclude(o => o.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email, ct);

        if (await CheckUserIsAllowedToLogin(dto, user, ct) is var checkUserAllowedResult
            && checkUserAllowedResult is not null)
            return checkUserAllowedResult;

        // We use SignInAsync instead of PasswordSignInAsync because SignInAsync contains custom logging logic
        await _signInManager.SignInAsync(user, new AuthenticationProperties(), "Password");

        var loggedUser = await _userDataService.Get(user.Id, ct);
        loggedUser.IsUserRequiredSetupTwoFactor = _user2FAService.IsUserRequiredSetup2FA(user);

        return new AuthResultDTO { UserId = user.Id, LoggedUser = loggedUser };
    }

    public async Task Login(RecoveryCodeDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId)
            ?? throw new UserNotExistsException();

        if (user.RecoveryCode != dto.Code)
        {
            var logMessage = "Failed to login with RecoveryCode: invalid RecoveryCode";
            await _loginAuditService.SaveLoginAuditAsync(user, logMessage);
            throw new LoginFailedException(ErrorMessages.WrongRecoveryCode);
        }

        if (user.TwoFactorEnabled)
        {
            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
                throw new ConflictException($"Unexpected error occurred while disabling 2FA.");
        }

        if (user.U2fEnabled)
            await DisableU2F(dto.UserId, cancellationToken);

        await _signInManager.SignInAsync(user, new AuthenticationProperties(), "RecoveryCode");
    }

    public async Task<SignInResult> TwoFactorAuthenticatorSignIn(string authenticatorCode) =>
        await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

    public async Task<SignInResult> TwoFactorRecoveryCodeSignIn(string recoveryCode) =>
        await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

    public async Task Logout()
    {
        var loggedUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        await _signInManager.SignOutAsync();
        if (loggedUser is not null)
        {
            // By updating the Authentication Security Stamp we make sure (depending on the configured
            // interval) that we reject/invalidate any authentication cookie using a previous stamp.
            await _userManager.UpdateAuthSecurityStampAsync(loggedUser);
        }
    }

    public async Task Activate(ResetPasswordDTO dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(dto.Email))
            throw new ObjectNotExistsException("The email cannot be sent. Email address is undefined.");

        var user = await _userManager.Users
            .Include(o => o.InvitationToken)
            .FirstOrDefaultAsync(x => x.Email == dto.Email, cancellationToken);

        if (user is null) throw new UserNotExistsException($"User with email {dto.Email} doesn't exist.");

        if (dto.Code != user.InvitationToken.Token)
            throw new BusinessException(ErrorMessages.ActivationCodeInvalid);

        if (user.InvitationToken.ExpirationDate is not null && user.InvitationToken.ExpirationDate < DateTime.Now)
            throw new BusinessException(ErrorMessages.InvitationExpired);

        // Check if user already passed activation
        if (user.AccountStatus != AccountStatus.Invited)
            throw new BusinessException(ErrorMessages.UserAlreadyActivated);

        var validationMessage = _securityService.CheckUsersNewPassword(user, dto.Password);
        if (!string.IsNullOrEmpty(validationMessage))
            throw new BusinessException(validationMessage);

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Code));
        var result = await _userManager.ResetInvitePasswordAsync(user, code, dto.Password);
        if (!result.Succeeded)
            throw new BusinessException(string.Join(Environment.NewLine,
                result.Errors.Select(x => x.Description).ToList()));

        user.AccountStatus = AccountStatus.Active;
        await _userManager.UpdateAsync(user);
        await RemoveInvitationToken(user, cancellationToken);

        await _securityService.SavePasswordToHistory(user, cancellationToken);
    }

    /// <summary>
    /// Starts the process of recovering a user password.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that we don't give an error back to the user when some conditions are
    /// met to avoid user-discoverability vulnerability (e.g., <strong>user is null</strong> => true).
    /// Also, we try to fulfill all requests within the same time span so attackers
    /// can't differentiate when a user exists or not by the time the request takes
    /// to complete.
    /// </para>
    /// <para>
    /// If the intended behavior is to be more "helpful" for the user (i.e., display an
    /// error) please consider discussing with <a href="mailto:james.peel@bbconsult.co.uk"></a>
    /// and/or <a href="mailto:duncan.forsyth@bbconsult.co.uk">Duncan Forsyth</a> before moving on
    /// changing the code.
    /// </para>
    /// </remarks>
    /// <param name="dto">DTO with recover information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task RecoverPassword(RecoverPasswordDTO dto, CancellationToken cancellationToken = default)
    {
        var sendResetEmail = true;
        var uniformWait = TimeSpan.FromSeconds(4);
        var recoverStartTime = DateTime.UtcNow;

        if (string.IsNullOrEmpty(dto.Email))
            sendResetEmail = false;

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || user.AccountStatus != AccountStatus.Active)
            sendResetEmail = false;

        if (sendResetEmail)
        {
            var activationToken = await SavePasswordResetToken(user, cancellationToken);
            await SendPasswordReset(user, activationToken.Token, cancellationToken);
        }

        var recoverTime = DateTime.UtcNow - recoverStartTime;
        if (recoverTime < uniformWait)
            await Task.Delay(uniformWait - recoverTime);
    }

    public async Task ConfirmEmail(ConfirmEmailDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.EmailConfirmationToken)
            .FirstOrDefaultAsync(x => x.Id == dto.UserId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        ConfirmEmailValidation(dto, user);

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
            throw new BusinessException(string.Join(Environment.NewLine, result.Errors.Select(x => x.Description).ToList()));

        user.AccountStatus = AccountStatus.Active;
        await _userManager.UpdateAsync(user);
        await RemoveEmailConfirmationToken(user, cancellationToken);
    }

    private static void ConfirmEmailValidation(ConfirmEmailDTO dto, User user)
    {
        if (!string.IsNullOrEmpty(dto.Code) && user.EmailConfirmationToken is null)
            throw new BusinessException("Activation code is invalid. It seems that you've already used this link.");

        if (string.IsNullOrEmpty(dto.Code) || dto.Code != user.EmailConfirmationToken?.Token)
            throw new BusinessException("Activation code is invalid.");

        if (user.EmailConfirmationToken?.ExpirationDate is not null && user.EmailConfirmationToken.ExpirationDate < DateTime.Now)
            throw new BusinessException("Email confirmation code has expired.");

        if (user.AccountStatus == AccountStatus.Suspended)
            throw new BusinessException("Account is suspended.");

        if (user.AccountStatus == AccountStatus.Deleted)
            throw new BusinessException("Account is deleted.");

        if (user.AccountStatus != AccountStatus.Unverified)
            throw new BusinessException("Only users with \"Unapproved\" status can be approved.");
    }

    public async Task Approve(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus != AccountStatus.Unapproved)
            throw new BusinessException("Only users with \"Unapproved\" status can be approved.");

        user.AccountStatus = AccountStatus.Unverified;
        await _userManager.UpdateAsync(user);
        await SaveEmailConfirmationToken(
            user,
            new ActivationToken
            {
                Token = await GenerateEmailConfirmationToken(user),
                ExpirationDate = DateTime.Now.AddDays(EmailConfirmationExpireInDays)
            },
            cancellationToken);

        await SendEmailConfirmation(user, user.EmailConfirmationToken.Token, cancellationToken);
    }

    public async Task ToggleLocking(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus != AccountStatus.Active &&
            user.AccountStatus != AccountStatus.Unverified &&
            user.AccountStatus != AccountStatus.Suspended)
            throw new BusinessException("Only users with \"Active\", \"Unverified\" or \"Suspended\" statuses can be locked or unlocked.");

        if (user.AccountStatus != AccountStatus.Suspended)
        {
            user.PreviousAccountStatus = user.AccountStatus;
            user.AccountStatus = AccountStatus.Suspended;
            await _userManager.SetLockoutEnabledAsync(user, true);
        }
        else
        {
            if (user.PreviousAccountStatus is null)
                throw new ConflictException("The previous account status is unset.");

            user.AccountStatus = (AccountStatus)user.PreviousAccountStatus;
            user.PreviousAccountStatus = null;
            await _userManager.SetLockoutEnabledAsync(user, false);
        }

        await _userManager.UpdateAsync(user);
    }

    public async Task ToggleDeleting(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus == AccountStatus.Deleted)
        {
            if (user.PreviousAccountStatus is null)
                throw new ConflictException("The previous account status is unset.");

            user.AccountStatus = (AccountStatus)user.PreviousAccountStatus;
            user.LockoutEnabled = false;
            user.PreviousAccountStatus = null;
        }
        else
        {
            if (user.AccountStatus != AccountStatus.Suspended)
                user.PreviousAccountStatus = user.AccountStatus;
            user.AccountStatus = AccountStatus.Deleted;
        }

        await _userManager.UpdateAsync(user);
    }

    public async Task EnableU2F(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        user.U2fEnabled = true;
        await _userManager.UpdateAsync(user);
    }

    public async Task DisableU2F(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        user.U2fEnabled = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task<string> Generate2FaOrU2FRecoveryCodes(string userId, bool isNeedNew, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) throw new UserNotExistsException();

        if (!user.TwoFactorEnabled && !user.U2fEnabled)
            throw new BusinessException(ErrorMessages.Disabled2FAOrU2F);

        if (string.IsNullOrEmpty(user.RecoveryCode) || isNeedNew)
        {
            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            user.RecoveryCode = string.Join("", codes.ToArray());
            await _userManager.UpdateAsync(user);
        }

        return user.RecoveryCode;
    }

    public async Task<U2FRegistrationRequestDTO> GenerateU2FDeviceRegistrationChallenge(string userId, string appUrl, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.AuthenticationRequests)
            .FirstOrDefaultAsync(o => o.Id == userId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        var startedRegistration = U2F.Core.Crypto.U2F.StartRegistration(appUrl);

        var authenticationRequest = new AuthenticationRequest
        {
            AppId = startedRegistration.AppId,
            Challenge = startedRegistration.Challenge,
            Version = U2F.Core.Crypto.U2F.U2FVersion
        };

        if (user.AuthenticationRequests is null)
            user.AuthenticationRequests = new List<AuthenticationRequest>();
        user.AuthenticationRequests.Add(authenticationRequest);
        await _userManager.UpdateAsync(user);

        return _mapper.Map<U2FRegistrationRequestDTO>(authenticationRequest);
    }

    public async Task RegisterU2FDevice(string userId, U2FRegistrationResponseDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.AuthenticationRequests)
            .Include(o => o.DeviceRegistrations)
            .FirstOrDefaultAsync(o => o.Id == userId, cancellationToken);
        if (user is null) throw new UserNotExistsException();

        if (user.AuthenticationRequests is null || user.AuthenticationRequests.Count == 0)
            throw new BusinessException(ErrorMessages.NoU2FChallenges);

        var registerResponse = new RegisterResponse(dto.RegistrationData, dto.ClientData);
        var lastAuthenticationRequest = user.AuthenticationRequests.Last();
        var startedRegistration =
            new StartedRegistration(lastAuthenticationRequest.Challenge, lastAuthenticationRequest.AppId);
        var deviceRegistration =
            U2F.Core.Crypto.U2F.FinishRegistration(startedRegistration, registerResponse);

        user.AuthenticationRequests.Clear();
        user.DeviceRegistrations.Add(new Device
        {
            AttestationCert = deviceRegistration.AttestationCert,
            Counter = Convert.ToInt32(deviceRegistration.Counter),
            CreatedOn = DateTime.Now,
            UpdatedOn = DateTime.Now,
            KeyHandle = deviceRegistration.KeyHandle,
            PublicKey = deviceRegistration.PublicKey
        });
        await _userManager.UpdateAsync(user);
    }

    public async Task<List<U2FRegisteredKeysDTO>> GenerateU2FDeviceAuthenticationChallenges(string userId, string appUrl, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.AuthenticationRequests)
            .Include(o => o.DeviceRegistrations)
            .FirstOrDefaultAsync(o => o.Id == userId, cancellationToken);
        if (user is null) throw new UserNotExistsException();

        var devices = user.DeviceRegistrations.Where(u => !u.IsCompromised).ToList();
        if (devices.Count == 0)
            throw new BusinessException("Suitable registered devices not found.");

        user.AuthenticationRequests.Clear();
        var challenge = U2F.Core.Crypto.U2F.GenerateChallenge();
        var serverChallenges = new List<U2FRegisteredKeysDTO>();

        foreach (var registeredDevice in devices)
        {
            user.AuthenticationRequests.Add(new AuthenticationRequest
            {
                AppId = appUrl,
                Challenge = challenge,
                KeyHandle = registeredDevice.KeyHandle.ByteArrayToBase64String(),
                Version = U2F.Core.Crypto.U2F.U2FVersion
            });
            serverChallenges.Add(new U2FRegisteredKeysDTO
            {
                Challenge = challenge,
                KeyHandle = registeredDevice.KeyHandle.ByteArrayToBase64String(),
                Version = U2F.Core.Crypto.U2F.U2FVersion
            });
        }

        await _userManager.UpdateAsync(user);
        return serverChallenges;
    }

    public async Task AuthenticateU2FDevice(U2FAuthenticationResponseDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.AuthenticationRequests)
            .Include(o => o.DeviceRegistrations)
            .FirstOrDefaultAsync(o => o.Id == dto.UserId);

        if (user is null) throw new UserNotExistsException();

        if (user.AuthenticationRequests is null || user.AuthenticationRequests.Count == 0)
            throw new BusinessException("There are no U2F authentication requests for this user.");

        var authenticateResponse = new AuthenticateResponse(
            dto.ClientData,
            dto.SignatureData,
            dto.KeyHandle
        );

        var device = user.DeviceRegistrations.FirstOrDefault(d =>
            d.KeyHandle.SequenceEqual(authenticateResponse.KeyHandle.Base64StringToByteArray()));
        if (device is null)
            throw new BusinessException("The registered device not found.");

        // User will have a authentication request for each device they have registered so get the one that matches the device key handle
        var authenticationRequest =
            user.AuthenticationRequests.FirstOrDefault(f => f.KeyHandle.Equals(authenticateResponse.KeyHandle));
        if (authenticationRequest is null)
            throw new ConflictException("There are no U2F authentication request for the corresponding found device.");

        var registration = new DeviceRegistration(
            device.KeyHandle,
            device.PublicKey,
            device.AttestationCert,
            Convert.ToUInt32(device.Counter)
        );

        var authentication = new StartedAuthentication(
            authenticationRequest.Challenge,
            authenticationRequest.AppId,
            authenticationRequest.KeyHandle
        );

        U2F.Core.Crypto.U2F.FinishAuthentication(authentication, authenticateResponse, registration);
        await _signInManager.SignInAsync(user, new AuthenticationProperties(), "U2F");

        user.AuthenticationRequests.Clear();
        device.Counter = Convert.ToInt32(registration.Counter);
        device.UpdatedOn = DateTime.Now;
        await _userManager.UpdateAsync(user);
    }

    [IgnoreLogging]
    public async Task<UserDTO> Impersonate(string impersonatingUserId, string impersonatedUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanImpersonate(impersonatingUserId, impersonatedUserId, cancellationToken))
            throw new BusinessException("You can not impersonate this user.");

        var impersonatingUser = await _userManager.FindByIdAsync(impersonatingUserId);
        var impersonatedUser = await _userManager.FindByIdAsync(impersonatedUserId);

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);
        userPrincipal.Identities.First()
            .AddClaim(new Claim(ClaimTypes.Impersonation.OriginalUserId, impersonatingUserId));
        userPrincipal.Identities.First().AddClaim(new Claim(ClaimTypes.Impersonation.OriginalUserName,
            $"{impersonatingUser.FirstName} {impersonatingUser.LastName}"));
        userPrincipal.Identities.First()
            .AddClaim(new Claim(ClaimTypes.Impersonation.IsImpersonating, bool.TrueString));

        await _httpContextAccessor.HttpContext.SignOutAsync();
        await _httpContextAccessor.HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal);

        return await _userDataService.Get(impersonatedUserId, cancellationToken);
    }

    [IgnoreLogging(true)]
    public async Task<UserDTO> StopImpersonation(ClaimsPrincipal userClaims, CancellationToken cancellationToken = default)
    {
        if (!userClaims.HasClaim(ClaimTypes.Impersonation.IsImpersonating, bool.TrueString))
            throw new BusinessException("User is not impersonating now.");

        var originalUserId = userClaims.FindFirstValue(ClaimTypes.Impersonation.OriginalUserId);
        if (string.IsNullOrEmpty(originalUserId))
            throw new ConflictException("Original identifier of the impersonating user is empty.");

        var originalUser = await _userManager.FindByIdAsync(originalUserId);
        if (originalUser is null)
            throw new ConflictException("User with original identifier of the impersonating user not found.");

        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(originalUser, false);

        return await _userDataService.Get(originalUser.Id, cancellationToken);
    }

    public async Task<List<PermissionDTO>> GetAllUserPermissions(string userId, CancellationToken cancellationToken)
    {
        var permissions = await
                (from ur in _context.Set<UserRole>()
                 join rp in _context.Set<RolePermission>() on ur.RoleId equals rp.RoleId
                 join p in _context.Set<Permission>() on rp.PermissionId equals p.Id
                 where ur.UserId == userId
                 select p)
            .Concat(
                from up in _context.Set<UserPermission>()
                join p in _context.Set<Permission>() on up.PermissionId equals p.Id
                where up.UserId == userId
                select p)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<PermissionDTO>>(permissions);
    }

    public async Task RefreshTwoFactorSetupClaims(User user = null, CancellationToken cancellationToken = default)
    {
        var userPrincipal = _httpContextAccessor.HttpContext.User;
        var identity = userPrincipal.Identities.FirstOrDefault();

        if (identity is null || !identity.Claims.Any()) return;

        var userId = _userManager.GetUserId(userPrincipal);
        user ??= await _userManager.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken: cancellationToken);
        if (user is null) return;

        var existingClaim = identity.FindFirst(ClaimTypes.Authentication.UserRequiredSetupTwoFactor);

        identity.TryRemoveClaim(existingClaim);

        identity.AddClaim(new Claim(ClaimTypes.Authentication.UserRequiredSetupTwoFactor,
            _user2FAService.IsUserRequiredSetup2FA(user).ToString()));

        await _httpContextAccessor.HttpContext.SignOutAsync();
        await _httpContextAccessor.HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal);
    }

    public async Task ResendEmailConfirmation(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(o => o.EmailConfirmationToken)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus != AccountStatus.Unverified)
            throw new BusinessException("Repeated sending of an email confirmation is possible only for users with the \"Unverified\" status.");

        await SaveEmailConfirmationToken(
            user,
            new ActivationToken
            {
                Token = await GenerateEmailConfirmationToken(user),
                ExpirationDate = DateTime.Now.AddDays(EmailConfirmationExpireInDays)
            },
            cancellationToken);

        await SendEmailConfirmation(user, user.EmailConfirmationToken.Token, cancellationToken);
    }

    public async Task NotifyUserOnEmailChanged(string userId, string oldEmail, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) throw new UserNotExistsException();

        if (user.AccountStatus == AccountStatus.Invited)
        {
            await ResendInvitation(userId, cancellationToken);
        }
        else
        {
            user.EmailConfirmed = false;
            user.AccountStatus = AccountStatus.Unverified;
            await _userManager.UpdateAsync(user);

            await SendEmailChangedNotification(user, oldEmail, cancellationToken);
            await ResendEmailConfirmation(userId, cancellationToken);
        }
    }

    public async Task SendTestEmail(string userEmail, int emailTemplateId, IFormFileCollection files, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null) throw new UserNotExistsException();

        var emailTemplate = await _emailTemplateService.Get(emailTemplateId);
        if (emailTemplate is null)
            throw new ConflictException("Email template not found.");


        var tagValues = new NameValueCollection
            {
                {"$DateTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("en-GB"))},
                {"$UserName", $"{user.FirstName} {user.LastName}"},
                {"$NewEmail", user.Email}
            };

        _emailTemplateService.BuildEmail(emailTemplate, tagValues);

        // Gets any branding
        var brand = await _dataService.Get<Branding, BrandingDTO>(query => query);
        EmailBrandInfo brandInfo = null;
        if (brand is not null && !string.IsNullOrEmpty(brand.EmailBody))
        {
            brandInfo = new EmailBrandInfo { Body = brand.EmailBody };
        }

        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, files?.ToArray(), brandInfo, to: userEmail);
    }

    public async Task CheckBrowserLoginAsync(AuthResultDTO result, string email, string fingerprint, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return;

        var oldFingerprint = user.LastLoginBrowserFingerprint;
        user.LastLoginBrowserFingerprint = fingerprint;
        await _userManager.UpdateAsync(user);

        if (string.IsNullOrEmpty(oldFingerprint) || string.IsNullOrEmpty(fingerprint) || result is null)
            return;

        var checkBrowserFingerprint = _loginSettings?.ShowNewBrowserLoginAlert ?? true;

        result.IsNewBrowserLogin =
            checkBrowserFingerprint &&
            string.Compare(fingerprint, oldFingerprint, CultureInfo.InvariantCulture, CompareOptions.None) != 0;
    }

    private async Task<AuthResultDTO> CheckUserIsAllowedToLogin(LoginDTO dto, User user, CancellationToken ct)
    {
        var currentIp = _httpContextAccessor.HttpContext.GetUserIp();

        // Check if IP been locked
        var lockoutIp = await _securityService.GetLongestActiveLockingByIp(currentIp, ct);
        if (lockoutIp is not null)
        {
            var lockoutIpTimeoutInSeconds = (lockoutIp.LockoutEnd - DateTime.Now).TotalSeconds;
            return new AuthResultDTO { LockoutIpEnabled = true, LockoutTimeoutInSeconds = (int?)lockoutIpTimeoutInSeconds };
        }

        // Defines whether there is any other credentials checking  feature is active (e.g. allowed IPs feature).
        var anyExtraCredentialsServiceActive = await _allowedIpService.IsServiceActive(ct);

        // To avoid exposing a user presence in system, we show a generalized message.
        // ! Please see https://cwe.mitre.org/data/definitions/203.html for a further explanation.
        // ! Note well: if this feature is removed, almost all site security reviews will insist that the feature is
        // ! added back in.”

        var generalizedMessage = anyExtraCredentialsServiceActive ?
            ErrorMessages.LoginFailureOrOtherCredentials :
            ErrorMessages.LoginFailureGeneralized;

        // Check if user with specified email exist
        if (user is null)
        {
            await _securityService.CheckIpLockOut(currentIp, ct);
            throw new WrongCredentialsException(generalizedMessage, $"User with email '{dto.Email}' doesn't exist.");
        }

        // Check if current IP address allowed for user
        if (!await _allowedIpService.IsIpAllowedForUser(currentIp, user.Id, ct))
            throw new LoginFailedException(generalizedMessage, $"IP address '{currentIp}' is not allowed for the user with email '{user.Id}'.");

        var isReCaptchaValid = await _recaptchaService.CheckReCaptchaAsync(dto.CaptchaResponse, ct);
        await _securityService.TryLockUserOnInvalidRecaptcha(user, isReCaptchaValid, ct);

        // Check if  user been locked
        if (await CheckUserLockoutOnLogin(user, ct) is var lockoutResult
            && lockoutResult is not null)
            return lockoutResult;

        // Check password
        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _securityService.AddFailedAttemptForUser(user, ct);
            throw new WrongCredentialsException(generalizedMessage, $"User with email '{dto.Email}' tried wrong password.");
        }

        // Validate account statuses
        ValidateAccountStatus(user.AccountStatus);

        if (await CheckIfPasswordRenewalRequired(dto, user, ct) is var seededAccountResult
            && seededAccountResult is not null)
            return seededAccountResult;

        if (await CheckSystemTesterOnLogin(dto, user) is var systemTesterResult
            && systemTesterResult is not null)
            return systemTesterResult;

        if (await CheckUserTwoFactorOnLogin(dto, user, isReCaptchaValid, ct) is var twoFactorResult
            && twoFactorResult is not null)
            return twoFactorResult;

        return null;
    }

    private async Task<AuthResultDTO> CheckUserLockoutOnLogin(User user, CancellationToken ct)
    {
        if (user.LockoutEnabled)
        {
            var settings = _settingsService.GetSettingsSection<FailedAttemptsPasswordSettings>();
            if (settings is null)
                throw new ConflictException("Settings for failed login attempts not found.");

            var isUserLocked = settings.UnlockTypeAccount == UnlockType.ResetPassword || user.AccountStatus == AccountStatus.Suspended;
            if (isUserLocked) throw new BusinessException("Your account is locked.");

            if (user.LockoutEnd > DateTime.UtcNow)
            {
                var lockoutTimeoutInSeconds = (user.LockoutEnd - DateTimeOffset.UtcNow)?.TotalSeconds;
                await _loginAuditService.SaveLoginAuditAsync(user, "Tried to log in, but his account is still locked");
                return new AuthResultDTO { UserId = user.Id, LockoutUserEnabled = true, LockoutTimeoutInSeconds = (int?)lockoutTimeoutInSeconds };
            }

            await _securityService.UnlockUser(user, ct);
        }

        return null;
    }

    private async Task<AuthResultDTO> CheckUserTwoFactorOnLogin(LoginDTO dto, User user, bool isReCaptchaValid, CancellationToken ct)
    {
        if (!user.U2fEnabled && !user.TwoFactorEnabled)
            return null;

        if (dto.TwoFactorCode is not null)
            return await TwoFactorAuthentication(user, dto, ct);

        if (await RequireUser2FAAuthOnLogin(user, isReCaptchaValid))
            return await TwoFactorAuthentication(user, dto, ct);

        await _loginAuditService.SaveLoginAuditAsync(user, "Passed the security check so no two-factor authentication is required");
        return null;
    }

    private bool IsSeededUserRequiredPasswordRenewal(User user)
    {
        if (_appEnvironmentService.IsLiveTypeEnvironment())
        {
            var seededUser = InitialUsers.GetAll().Find(x => x.Email == user.Email);

            if (seededUser is not null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var newPassword = _securityService.GetHashedPassword(seededUser.Password);
                var verifyRes = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, newPassword);
                var userPasswordAndSeededPasswordMatch = verifyRes == PasswordVerificationResult.Success;

                // If password passed from the form matches the seeded password then we need renew it
                return userPasswordAndSeededPasswordMatch;
            }
        }

        return false;
    }
    /// <summary>
    /// Check if user needs password renewal on login.
    /// For example, when an account seeded by the app on the very first start and the user first time logs in with a default password
    /// </summary>
    private async Task<AuthResultDTO> CheckIfPasswordRenewalRequired(LoginDTO dto, User user, CancellationToken ct)
    {
        if (IsSeededUserRequiredPasswordRenewal(user))
        {
            // User has entered a password initially seeded by the app and now will be asked to renew it using a
            // standard reset password procedure
            if (string.IsNullOrEmpty(dto.NewPassword))
            {
                return new AuthResultDTO
                {
                    UserId = user.Id,
                    PasswordResetRequired = true,
                    PasswordResetRequest = new PasswordResetRequestDTO
                    {
                        PasswordResetCode = (await SavePasswordResetToken(user, ct)).Token,
                        Reason = PasswordResetRequestReason.InitialAccountReset
                    }

                };
            }
        }

        return null;
    }

    /// <summary>
    /// Check if user is tester
    /// </summary>
    private async Task<AuthResultDTO> CheckSystemTesterOnLogin(LoginDTO dto, User user)
    {
        var isSystemTester = user.UserRoles.Any(x => x.Role.Name == Roles.SystemTester) &&
                   string.IsNullOrEmpty(dto.RealLastName) &&
                   string.IsNullOrEmpty(dto.RealFirstName) &&
                   string.IsNullOrEmpty(dto.RealEmail);

        if (isSystemTester)
        {
            await _loginAuditService.SaveLoginAuditAsync(user, "requires a tester data");
            return new AuthResultDTO { UserId = user.Id, IsSystemTester = true };
        }

        return null;
    }

    private async Task SendEmailChangedNotification(User user, string oldEmail, CancellationToken cancellationToken = default)
    {
        if (_emailSender is null) return;

        var emailTemplate = await _emailTemplateService.GetByCode("ChangeEmail", cancellationToken);
        if (emailTemplate is null)
            throw new ConflictException("Email template for the notification of an email changing not found.");

        var tagValues = new NameValueCollection
            {
                {"$UserName", $"{user.FirstName} {user.LastName}"},
                {"$OldEmail", oldEmail},
                {"$NewEmail", user.Email}
            };
        _emailTemplateService.BuildEmail(emailTemplate, tagValues);

        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: oldEmail);
    }

    private int InvitationExpireDays
        => _settingsService
            .GetSettingsSection<RegistrationSettings>()?
            .UserInvitationExpireInDays ?? RegistrationSettings.DefaultUserInvitationExpireInDays;

    private int EmailConfirmationExpireInDays
        => _settingsService
            .GetSettingsSection<RegistrationSettings>()?
            .EmailConfirmationExpireInDays ?? RegistrationSettings.DefaultEmailConfirmationExpireInDays;

    private int PasswordResetExpireInDays
        => _settingsService
            .GetSettingsSection<UserPasswordSettings>()?
            .PasswordResetTokenExpireInDays ?? UserPasswordSettings.DefaultPasswordResetExpireInDays;

    #region private helpers
    private async Task<bool> RequireUser2FAAuthOnLogin(User user, bool isReCaptchaValid)
    {
        // Check that user did set up 2FA in profile
        if (!user.U2fEnabled && !user.TwoFactorEnabled) return false;

        // ReCaptcha validity check
        if (!isReCaptchaValid)
            await _loginAuditService.SaveLoginAuditAsync(user, "Did not pass security check (ReCaptcha rating less than specified), therefore two-factor authentication is required");

        var lastSuccessfulLoginAudit = await _loginAuditService.GetLastSuccessfulLoginAuditAsync(user.Email);
        if (lastSuccessfulLoginAudit is null) return true;

        // IP change check
        var currentIp = _httpContextAccessor.HttpContext.GetUserIp();
        var isIpChanged = currentIp != lastSuccessfulLoginAudit.Ip;
        if (isIpChanged)
            await _loginAuditService.SaveLoginAuditAsync(user, "Did not pass security check (IP changed), therefore two-factor authentication is required");

        // 2FA ignore interval check
        var twoFactorSettings = _settingsService.GetSettingsSection<TwoFactorSettings>();
        var twoFactorIgnoreInterval = new TimeSpan(0, twoFactorSettings.AuthDurationMinutesOnLogin.GetValueOrDefault(), 0);
        var intervalSinceLastAuthentication = DateTimeOffset.UtcNow - lastSuccessfulLoginAudit.Datetime;
        var is2FaIgnoreIntervalExpired = intervalSinceLastAuthentication > twoFactorIgnoreInterval;
        if (is2FaIgnoreIntervalExpired)
            await _loginAuditService.SaveLoginAuditAsync(user, "Did not pass security check (2 factor Ignore interval expired), therefore two-factor authentication is required");

        // logged out event check
        var lastSignedOutAudit = await _loginAuditService.GetLastSignedOutAuditAsync(user.Email);
        var isUserExplicitlyLoggedOut = (lastSignedOutAudit is not null) && (lastSuccessfulLoginAudit.Datetime.Ticks < lastSignedOutAudit.Datetime.Ticks);
        if (isUserExplicitlyLoggedOut)
            await _loginAuditService.SaveLoginAuditAsync(user, "The user has explicitly logged out, therefore two-factor authentication is required");

        return is2FaIgnoreIntervalExpired || isIpChanged || !isReCaptchaValid || isUserExplicitlyLoggedOut;
    }

    private async Task<AuthResultDTO> TwoFactorAuthentication(User user, LoginDTO dto, CancellationToken cancellationToken = default)
    {
        var result = new AuthResultDTO() { UserId = user.Id };

        // U2F
        if (user.U2fEnabled)
        {
            var deviceChallenges = await GenerateU2FDeviceAuthenticationChallenges(user.Id, DomainUrl, cancellationToken);

            var u2FAuthenticationRequest = new U2FAuthenticationRequestDTO
            {
                AppId = DomainUrl,
                Version = deviceChallenges[0].Version,
                Challenge = deviceChallenges[0].Challenge,
                RegisteredKeys = deviceChallenges
            };

            result.U2FEnabled = true;
            result.U2FAuthenticationRequest = u2FAuthenticationRequest;

            return result;
        }

        if (string.IsNullOrEmpty(dto.TwoFactorCode))
        {
            var signInResult = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);

            if (!signInResult.RequiresTwoFactor) return result;
            result.AuthenticatorEnabled = true;

            return result;
        }

        var authenticatorCode = dto.TwoFactorCode
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty);

        var twoFactorSignInResult = await TwoFactorAuthenticatorSignIn(authenticatorCode);

        if (!twoFactorSignInResult.Succeeded)
            throw new LoginFailedException("Invalid authenticator code.");
        else
            await _loginAuditService.SaveLoginAuditAsync(user, LogMessages.Passed2FACode);

        result.LoggedUser = await _userDataService.Get(user.Id, cancellationToken);
        result.LoggedUser.IsUserRequiredSetupTwoFactor = _user2FAService.IsUserRequiredSetup2FA(user);

        return result;
    }

    private async Task SaveInvitationToken(User user, ActivationToken activationToken, CancellationToken cancellationToken)
    {
        if (activationToken is null)
        {
            if (user.InvitationToken is not null)
                await RemoveInvitationToken(user, cancellationToken);
        }
        else
        {
            if (user.InvitationToken is null)
            {
                user.InvitationToken = activationToken;
            }
            else
            {
                user.InvitationToken.Token = activationToken.Token;
                user.InvitationToken.ExpirationDate = activationToken.ExpirationDate;
            }

            await _userManager.UpdateAsync(user);
        }
    }

    private async Task RemoveInvitationToken(User user, CancellationToken cancellationToken)
    {
        var oldTokenId = user.InvitationToken.Id;
        user.InvitationTokenId = null;
        user.InvitationToken = null;
        await _userManager.UpdateAsync(user);
        await _dataService.Delete<ActivationToken>(oldTokenId, cancellationToken);
    }

    private async Task<ActivationToken> SavePasswordResetToken(User user, CancellationToken ct)
    {
        var activationToken = new ActivationToken
        {
            ExpirationDate = DateTime.Now.AddDays(PasswordResetExpireInDays),
            Token = await GeneratePasswordResetToken(user)
        };

        await SavePasswordResetToken(user, activationToken, ct);
        return activationToken;
    }

    private async Task SavePasswordResetToken(User user, ActivationToken activationToken, CancellationToken cancellationToken)
    {
        if (activationToken is null)
        {
            if (user.PasswordResetToken is not null)
                await RemovePasswordResetToken(user, cancellationToken);
        }
        else
        {
            if (user.PasswordResetToken is null)
            {
                user.PasswordResetToken = activationToken;
            }
            else
            {
                user.PasswordResetToken.Token = activationToken.Token;
                user.PasswordResetToken.ExpirationDate = activationToken.ExpirationDate;
            }

            await _userManager.UpdateAsync(user);
        }
    }

    private async Task RemovePasswordResetToken(User user, CancellationToken cancellationToken)
    {
        var oldTokenId = user.PasswordResetToken.Id;
        user.PasswordResetTokenId = null;
        user.PasswordResetToken = null;
        await _userManager.UpdateAsync(user);
        await _dataService.Delete<ActivationToken>(oldTokenId, cancellationToken);
    }

    private async Task SaveEmailConfirmationToken(User user, ActivationToken activationToken, CancellationToken cancellationToken)
    {
        if (activationToken is null)
        {
            if (user.EmailConfirmationToken is not null)
                await RemoveEmailConfirmationToken(user, cancellationToken);
        }
        else
        {
            if (user.EmailConfirmationToken is null)
            {
                user.EmailConfirmationToken = activationToken;
            }
            else
            {
                user.EmailConfirmationToken.Token = activationToken.Token;
                user.EmailConfirmationToken.ExpirationDate = activationToken.ExpirationDate;
            }

            await _userManager.UpdateAsync(user);
        }
    }

    private async Task RemoveEmailConfirmationToken(User user, CancellationToken cancellationToken)
    {
        var oldTokenId = user.EmailConfirmationToken.Id;
        user.EmailConfirmationTokenId = null;
        user.EmailConfirmationToken = null;
        await _userManager.UpdateAsync(user);
        await _dataService.Delete<ActivationToken>(oldTokenId, cancellationToken);
    }

    private async Task SendInvitation(User user, string token, CancellationToken cancellationToken = default)
    {
        if (_httpContextAccessor.HttpContext is null || _emailSender is null) return;

        // Generate callbackUrl
        var callbackUrl = $"{DomainUrl}/account/activate?userId={user.Id}&code={token}";

        // Send email
        var emailTemplate = await _emailTemplateService.GetByCode("UserInvitation", cancellationToken);
        if (emailTemplate is null)
            throw new ConflictException("Email template for the user invitation not found.");

        var tagValues = new NameValueCollection
            {
                {"$UserName", $"{user.FirstName} {user.LastName}"},
                {"$CallbackUrl", callbackUrl}
            };
        _emailTemplateService.BuildEmail(emailTemplate, tagValues);

        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: user.Email);
    }

    private async Task SendEmailConfirmation(User user, string token, CancellationToken cancellationToken = default)
    {
        if (_httpContextAccessor.HttpContext is null || _emailSender is null) return;

        // Generate callbackUrl
        var callbackUrl = $"{DomainUrl}/account/confirmemail?userId={user.Id}&code={token}";

        // Send email
        var emailTemplate = await _emailTemplateService.GetByCode("EmailConfirmation", cancellationToken);
        if (emailTemplate is null)
            throw new ConflictException("Email template for the email confirmation not found.");

        var tagValues = new NameValueCollection
            {
                {"$UserName", $"{user.FirstName} {user.LastName}"},
                {"$CallbackUrl", callbackUrl},
            };
        _emailTemplateService.BuildEmail(emailTemplate, tagValues);

        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: user.Email);
    }

    private async Task SendPasswordReset(User user, string token, CancellationToken cancellationToken = default)
    {
        if (_httpContextAccessor.HttpContext is null || _emailSender is null) return;

        // Generate callbackUrl
        var callbackUrl = $"{DomainUrl}/account/resetpassword?userId={user.Id}&code={token}";

        // Send email
        var emailTemplate = await _emailTemplateService.GetByCode("ResetPassword", cancellationToken);
        if (emailTemplate is null)
            throw new ConflictException("Email template for the password reset not found.");

        var tagValues = new NameValueCollection
            {
                {"$UserName", $"{user.FirstName} {user.LastName}"},
                {"$CallbackUrl", callbackUrl},
                {"$IpAddress", _httpContextAccessor.HttpContext.GetUserIp()}
            };
        _emailTemplateService.BuildEmail(emailTemplate, tagValues);

        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: user.Email);
    }

    private async Task SendPasswordChangedNotification(string email, CancellationToken cancellationToken = default)
    {
        if (_emailSender is null) return;

        if (string.IsNullOrEmpty(email))
            throw new ObjectNotExistsException("Email with a password changed notification is empty.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            throw new UserNotExistsException("User which password is being changed not found.");

        var emailTemplate = await _emailTemplateService.GetByCode("PasswordChanged", cancellationToken);
        if (emailTemplate is null)
            throw new ConflictException("Email template for \"Password Changed\" notification not found.");

        var tagValues = new NameValueCollection
            {
                {"$UserName", $"{user.FirstName} {user.LastName}"}
            };

        _emailTemplateService.BuildEmail(emailTemplate, tagValues);
        await _emailSender.SendEmail(emailTemplate.Subject, emailTemplate.Body, from: emailTemplate.From, to: email);
    }

    private async Task<string> GeneratePasswordResetToken(User user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    private async Task<string> GenerateEmailConfirmationToken(User user)
    {
        try
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }
        catch
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }
    }

    /// <summary>
    /// Defines whether a self registered user needs to be approved by admin to complete his registration in the system.
    ///
    /// If you need to add the approval option, then rewrite this method. For example, the method could read a flag setting
    /// ([v] Admin approval  required on registration) from the system configuration settings.
    /// </summary>
    private static bool AdminApprovalRequired() => true;

    private static void ValidateAccountStatus(AccountStatus accountStatus)
    {
        switch (accountStatus)
        {
            // Check that user is invited
            case AccountStatus.Invited:
                throw new BusinessException("An invitation has not been accepted yet.");
            // Check that user is approved
            case AccountStatus.Unapproved:
                throw new BusinessException("Account is not approved yet.");
            // Check that user is verified
            case AccountStatus.Unverified:
                throw new BusinessException("Email address is not verified yet.");
            // Check that user is not suspended
            case AccountStatus.Suspended:
                throw new BusinessException("Account is suspended.");
            // Check that user is not deleted
            case AccountStatus.Deleted:
                throw new BusinessException("Account is deleted.");
            default:
                break;
        }
    }
    #endregion

    public static class ErrorMessages
    {
        /// <summary>
        /// This message is used for the login failure cases when we avoid revealing a fact of user presence in the system.
        /// </summary>
        public static readonly string LoginFailureGeneralized = "Incorrect user name or password.";
        public static readonly string LoginFailureOrOtherCredentials = "Incorrect user name, password, or other credential.";
        public static readonly string RecoveryNotFoundForUser = "Recovery code not found for the user.";
        public static readonly string RecoveryExpired = "Recovery code has expired.";
        public static readonly string RecoveryInvalid = "Recovery code is invalid.";
        public static readonly string ActivationCompleted = "Activation already completed.";
        public static readonly string ActivationCodeInvalid = "Activation code is invalid.";
        public static readonly string InvitationNotFoundForUser = "Invitation not found for the user.";
        public static readonly string InvitationExpired = "Sorry, your link has expired. Please ask your administrator to resend the invitation.";
        public static readonly string ImpersonatingUserNotFound = "Impersonating user doesn't exist.";
        public static readonly string ImpersonatedUserNotFound = "Impersonated user doesn't exist.";
        public static readonly string EmailExistForDeleted = "User with this email already exists and has 'deleted' status. You should undelete the user on the user details page instead.";
        public static readonly string EmailExist = "User with this email already exists";
        public static readonly string UserNotActivatedForEmail = "User with email u.Email is not activated";
        public static readonly string UserAlreadyActivated = "User is already active.";
        public static readonly string WrongRecoveryCode = "Wrong recovery code";
        public static readonly string UndefinedEmail = "The email cannot be sent. Email address is undefined.";
        public static readonly string Disabled2FAOrU2F = "Cannot generate recovery codes for user as he does not have 2FA or U2F enabled";
        public static readonly string NoU2FChallenges = "There are no U2F registration challenges for this user";
    }
}
