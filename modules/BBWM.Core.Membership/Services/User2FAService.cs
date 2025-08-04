using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Exceptions;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.SystemSettings;
using BBWM.SystemSettings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Encodings.Web;

namespace BBWM.Core.Membership.Services;

/// <summary>
/// TODO: to move bit by bit 2FA/U2F related code out from UserService into this one
/// </summary>
public class User2FAService : IUser2FAService
{
    private readonly UserManager<User> _userManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly ILoginAuditService _loginAuditService;
    private readonly ISettingsService _settingsService;
    private readonly UserLoginSettings _loginSettings;

    public User2FAService(
        UserManager<User> userManager,
        UrlEncoder urlEncoder,
        IOptionsSnapshot<UserLoginSettings> loginSettingsOptions,
        ILoginAuditService loginAuditService,
        ISettingsService settingsService
    )
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
        _loginAuditService = loginAuditService;
        _settingsService = settingsService;
        _loginSettings = loginSettingsOptions?.Value;
    }

    public static string Format2FACode(string code) =>
        code.Replace(" ", string.Empty).Replace("-", string.Empty);

    public async Task<Enabling2FADTO> Get2FAEnablingData(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotExistsException();

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!is2faEnabled)
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        return new Enabling2FADTO
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
        };
    }

    public async Task Enable2FA(Enabling2FADTO dto, string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotExistsException();

        // Strip spaces and hyphens
        var verificationCode = Format2FACode(dto.Code);
        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (is2FaTokenValid)
            await _loginAuditService.SaveLoginAuditAsync(user, LogMessages.Passed2FACode);
        else 
            throw new BusinessException("Verification code is invalid.");

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            throw new ConflictException($"Unexpected error occurred while enabling 2FA.");
    }

    public async Task Disable2FA(string userId, string code, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UserNotExistsException();

        var verificationCode = Format2FACode(code);
        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (is2FaTokenValid)
            await _loginAuditService.SaveLoginAuditAsync(user, LogMessages.Passed2FACode);
        else 
            throw new BusinessException("Verification code is invalid.");

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
            throw new ConflictException($"Unexpected error occurred while disabling 2FA.");
    }

    public bool IsUserRequiredSetup2FA(User user)
    {
        var globalTwoFactorMode = _settingsService.GetSettingsSection<TwoFactorSettings>().MandatoryMode;
        var userRoles = user.UserRoles;

        var isSetupTwoFactorCheckRequired = false;

        // Users with SuperAdmin role is excluded from being demanded to set up 2FA in order not to block the whole site's
        // UI. At least SuperAdmin user should access site pages.
        if (userRoles.All(x => x.Role.Name != Roles.SuperAdminRole))
        {
            isSetupTwoFactorCheckRequired = globalTwoFactorMode switch
            {
                TwoFactorMandatoryMode.Mandatory => true,
                TwoFactorMandatoryMode.MandatoryForSpecificRoles =>
                    userRoles.Any() && userRoles.Any(x => x.Role.AuthenticatorRequired),
                TwoFactorMandatoryMode.Optional => false,
                _ => false
            };
        }

        return isSetupTwoFactorCheckRequired && !(user.U2fEnabled || user.TwoFactorEnabled);
    }

    public async Task Verify2FACode(Checking2FADTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId)
            ?? throw new UserNotExistsException();

        // Strip spaces and hyphens
        var verificationCode = Format2FACode(dto.Code);
        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2FaTokenValid)
            throw new BusinessException("Verification code is invalid.");
        else
            await _loginAuditService.SaveLoginAuditAsync(user, LogMessages.Passed2FACode);
    }


    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
        => string.Format("otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            _urlEncoder.Encode(_loginSettings?.TwoFaAppName
                ?? UserLoginSettings.DefaultTwoFaAppName), _urlEncoder.Encode(email), unformattedKey);
}
