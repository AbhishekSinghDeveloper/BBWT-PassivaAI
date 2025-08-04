using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BBWM.AWS.EventBridge.Api;

[AttributeUsage(AttributeTargets.Class)]
public class AuthorizeEventBridgeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var services = context.HttpContext.RequestServices;
        var settings = services?
            .GetService<IOptionsSnapshot<AwsEventBridgeSettings>>()?.Value;

        if (context.HttpContext.Request.Headers.TryGetValue(settings.AuthHeader ?? "", out var values) &&
            values.Count > 0)
        {
            var apiKey = values.FirstOrDefault();
            if (apiKey == settings.APIKey) { return; }
        }

        context.Result = new UnauthorizedResult();
    }
}
