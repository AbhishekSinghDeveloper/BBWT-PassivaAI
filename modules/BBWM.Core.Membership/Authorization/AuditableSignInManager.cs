using BBWM.Core.Membership.Constants;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace BBWM.Core.Membership.Authorization;

public class AuditableSignInManager : SignInManager<User>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILoginAuditService _loginAuditService;
    private readonly UserManager<User> _userManager;

    public AuditableSignInManager(
        IAuthenticationSchemeProvider authenticationSchemeProvider,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        IUserConfirmation<User> userConfirmation,
        ILogger<AuditableSignInManager> logger,
        ILoginAuditService loginAuditService,
        IHttpContextAccessor contextAccessor,
        UserManager<User> userManager) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, authenticationSchemeProvider, userConfirmation)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _loginAuditService = loginAuditService ?? throw new ArgumentNullException(nameof(loginAuditService));
    }

    public override async Task<SignInResult> PasswordSignInAsync(User user, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        await _loginAuditService.SaveLoginAuditAsync(user, GenerateSignInResultMessage(result));
        return result;
    }

    public override async Task SignInAsync(User user, AuthenticationProperties authenticationProperties, string authenticationMethod = null)
    {
        await base.SignInAsync(user, authenticationProperties, authenticationMethod);
        var logMessage = authenticationMethod is null ? LogMessages.LoggedIn : $"{LogMessages.LoggedIn} with {authenticationMethod}";
        await _loginAuditService.SaveLoginAuditAsync(user, logMessage);
    }

    public override async Task SignOutAsync()
    {
        await base.SignOutAsync();
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
        await _loginAuditService.SaveLoginAuditAsync(user, LogMessages.SignedOut);
    }

    public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient)
    {
        var user = await GetTwoFactorUserAsync();
        if (user is null) return SignInResult.Failed;

        var result = await base.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
        await _loginAuditService.SaveLoginAuditAsync(user, $"{GenerateSignInResultMessage(result)} with 2FA");
        return result;
    }

    #region private helpers

    private static string GenerateSignInResultMessage(SignInResult signInResult)
    {
        var signInResultMessage = signInResult.ToString();
        return signInResultMessage switch
        {
            "Succeeded" => LogMessages.LoggedIn,
            "Failed" => LogMessages.FailedToLogin,
            "RequiresTwoFactor" => LogMessages.RequiresTwoFactor,
            _ => signInResultMessage
        };
    }

    private async Task<User> GetTwoFactorUserAsync()
    {
        var result = await Context.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
        var userId = result?.Principal?.FindFirstValue(ClaimTypes.Name);

        return await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
    }

    #endregion
}
