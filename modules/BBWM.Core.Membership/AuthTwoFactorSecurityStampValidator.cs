using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Security.Claims;

namespace BBWM.Core.Membership;

/// <summary>
/// Custom Two Factor Security Stamp validator that validates the Authentication Security
/// Stamp on the <typeparamref name="TUser"/>.
/// </summary>
/// <remarks>
/// See <see cref="AuthSecurityStampValidator{TUser}"/>.
/// </remarks>
/// <typeparam name="TUser">User type</typeparam>
public class AuthTwoFactorSecurityStampValidator<TUser> : AuthSecurityStampValidator<TUser>, ITwoFactorSecurityStampValidator
    where TUser : User
{
    public AuthTwoFactorSecurityStampValidator(
        IOptions<AuthSecurityStampValidatorOptions> options,
        SignInManager<TUser> signInManager,
        UserManager<TUser> userManager,
        ISystemClock clock,
        ILoggerFactory logger)
        : base(options, signInManager, userManager, clock, logger)
    { }

    protected override Task<TUser> VerifySecurityStamp(ClaimsPrincipal principal)
        => SignInManager.ValidateTwoFactorSecurityStampAsync(principal);

    protected override Task SecurityStampVerified(TUser user, CookieValidatePrincipalContext context)
        => Task.CompletedTask;
}
