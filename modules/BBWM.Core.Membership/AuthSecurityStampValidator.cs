using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Globalization;
using System.Security.Claims;

using BbwtClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership;

/// <summary>
/// Custom Security Stamp Validator that validates the Authentication Security Stamp
/// on the <typeparamref name="TUser"/>.
/// </summary>
/// <remarks>
/// <para>
/// The Authentication Security Stamp is a value kept on the <typeparamref name="TUser"/> in
/// order to be able to reject/invalidate any authentication cookie after the <typeparamref name="TUser"/>
/// logs out. The check is made by comparing the value stored in the <typeparamref name="TUser"/>'s
/// claims and the one in the database after the configured interval has expired.
/// </para>
/// <para>
/// The Authentication Security Stamp should be generated during user creation and only updated when the
/// <typeparamref name="TUser"/> logs out.
/// </para>
/// </remarks>
/// <typeparam name="TUser">User type</typeparam>
public class AuthSecurityStampValidator<TUser> : SecurityStampValidator<TUser>
    where TUser : User
{
    public AuthSecurityStampValidator(
        IOptions<AuthSecurityStampValidatorOptions> options,
        SignInManager<TUser> signInManager,
        UserManager<TUser> userManager,
        ISystemClock clock,
        ILoggerFactory logger)
        : base(options, signInManager, clock, logger)
    {
        AuthSecurityValidationInterval = options.Value?.AuthValidationInterval ?? TimeSpan.FromSeconds(90);
        UserManager = userManager;
    }

    public TimeSpan AuthSecurityValidationInterval { get; }

    public UserManager<TUser> UserManager { get; }

    public override async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var issuedUtc = context.Properties.IssuedUtc;
        var currentUtc = DateTimeOffset.UtcNow;

        if (Clock is not null)
            currentUtc = Clock.UtcNow;

        var validateAuth = issuedUtc is null;
        if (issuedUtc is not null)
            validateAuth = currentUtc.Subtract(issuedUtc.Value) > AuthSecurityValidationInterval;

        if (validateAuth && !await ValidateAuthSecurityStampAsync(context.Principal))
        {
            Logger.LogInformation("Auth security stamp validation failed, rejecting cookie.");
            context.RejectPrincipal();
            await SignInManager.SignOutAsync();
            await SignInManager.Context.SignOutAsync(IdentityConstants.TwoFactorRememberMeScheme);
        }
        else
            await base.ValidateAsync(context);
    }

    private async Task<bool> ValidateAuthSecurityStampAsync(ClaimsPrincipal principal)
    {
        var user = await UserManager.GetUserAsync(principal);
        var authSecurityStamp = principal.FindFirstValue(BbwtClaimTypes.Authentication.AuthSecurityStamp);

        if (user is null || string.IsNullOrEmpty(authSecurityStamp))
            return false;

        return string.Compare(
            authSecurityStamp,
            user.AuthSecurityStamp,
            false,
            CultureInfo.InvariantCulture) == 0;
    }
}
