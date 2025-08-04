using BBWM.Core.Membership.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace BBWM.Core.Membership.Filters;

public class User2FaAccessFilter : IAsyncAuthorizationFilter
{
    private readonly ILogger<User2FaAccessFilter> _logger;

    public User2FaAccessFilter(ILogger<User2FaAccessFilter> logger)
    {
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        return !IsSetupTwoFactorCheckRequired(context) || !context.HttpContext.User.IsUserRequiredSetupTwoFactor()
            ? Task.CompletedTask
            : ResourceForbiddenAsync(context);
    }

    #region private helpers

    private static bool IsSetupTwoFactorCheckRequired(AuthorizationFilterContext filterContext)
    {
        if (!filterContext.HttpContext.User.Identity.IsAuthenticated) return false;

        var hasAllowAnonymous = filterContext.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
        var hasIgnoreSetup2FaCheck = filterContext.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(IgnoreSetup2FaCheckAttribute));

        return !hasAllowAnonymous && !hasIgnoreSetup2FaCheck;
    }

    private Task ResourceForbiddenAsync(AuthorizationFilterContext filterContext)
    {
        _logger.LogInformation($"User with name {filterContext.HttpContext.User.Identity.Name} tried to get an access to a secure resource without enabled 2FA");
        filterContext.Result = new ObjectResult("You must to set two factor authentication to access this resource")
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        return Task.CompletedTask;
    }

    #endregion
}
