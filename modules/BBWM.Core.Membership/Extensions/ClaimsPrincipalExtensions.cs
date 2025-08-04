using System.Security.Claims;

using ClaimTypes = BBWM.Core.Membership.Model.ClaimTypes;

namespace BBWM.Core.Membership.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool IsUserRequiredSetupTwoFactor(this ClaimsPrincipal user)
    {
        var userRequiresSetupTwoFactorClaim = user.FindFirst(ClaimTypes.Authentication.UserRequiredSetupTwoFactor)?.Value;
        var userImpersonatingClaim = user.FindFirst(ClaimTypes.Impersonation.IsImpersonating)?.Value;
        bool.TryParse(userRequiresSetupTwoFactorClaim, out var userRequiresSetupTwoFactor);
        bool.TryParse(userImpersonatingClaim, out var userImpersonating);
        return !userImpersonating && userRequiresSetupTwoFactor;
    }
}
