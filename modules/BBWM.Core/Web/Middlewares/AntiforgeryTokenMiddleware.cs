using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BBWM.Core.Web.Middlewares;

public class AntiforgeryTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;
    private readonly IEnumerable<string> _tokenRefreshingSegments;

    public AntiforgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery, IEnumerable<string> tokenRefreshingSegments)
    {
        _next = next;
        _antiforgery = antiforgery;
        _tokenRefreshingSegments = tokenRefreshingSegments;
    }

    public Task Invoke(HttpContext context)
    {
        if (_tokenRefreshingSegments.Any(o => context.Request.Path.StartsWithSegments(o, StringComparison.OrdinalIgnoreCase)))
        {
            var tokens = _antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false, Secure = true });
        }

        return _next(context);
    }
}

public static class AntiforgeryTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseAntiforgeryToken(this IApplicationBuilder builder, IEnumerable<string> tokenRefreshingSegments)
    {
        return builder.UseMiddleware<AntiforgeryTokenMiddleware>(tokenRefreshingSegments);
    }
}
