using Microsoft.AspNetCore.Http;

using System.Security.Claims;

namespace BBWM.Core.Web.Extensions;

public static class HttpContextExtensions
{
    public static string GetDomainUrl(this HttpContext httpContext)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";

    public static string GetUserEmail(this HttpContext httpContext)
        => httpContext.User.FindFirstValue(ClaimTypes.Email);

    public static string GetUserId(this HttpContext httpContext)
        => httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string GetUserIp(this HttpContext httpContext)
    {
        var header = httpContext.Request?.Headers["X-Real-IP"];
        if (header?.Any() ?? false)
            return header?.First();

        return httpContext.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
