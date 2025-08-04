using Microsoft.AspNetCore.Builder;

namespace BBWM.Core.Web.CookieAuth;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseDisableSlidingExpirationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DisableSlidingExpirationMiddleware>();
    }
}
