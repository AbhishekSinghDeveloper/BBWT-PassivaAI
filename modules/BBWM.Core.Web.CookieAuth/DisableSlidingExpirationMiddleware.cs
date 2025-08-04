using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace BBWM.Core.Web.CookieAuth;

public class DisableSlidingExpirationMiddlewareOptions
{
    public string AuthCookieName { get; set; }
}

public class DisableSlidingExpirationMiddleware
{
    private readonly RequestDelegate _next;

    private readonly string _authCookieName;

    public DisableSlidingExpirationMiddleware(RequestDelegate next, IOptionsSnapshot<CookieAuthSettings> cookieAuthConfig)
    {
        _next = next;
        _authCookieName = cookieAuthConfig?.Value?.CookieName;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!string.IsNullOrEmpty(_authCookieName))
        {
            context.Response.OnStarting(state =>
            {
                if (context.Items.ContainsKey(DoNotResetAuthCookieAttribute.Name))
                {
                    var response = (HttpResponse)state;

                    // Omit Set-Cookie header with the offending cookie name
                    var cookieHeader = response.Headers[HeaderNames.SetCookie]
                    .Where(s => !s.Contains(_authCookieName))
                    .Aggregate(new StringValues(), (current, s) => StringValues.Concat(current, s));

                    response.Headers[HeaderNames.SetCookie] = cookieHeader;
                }
                return Task.CompletedTask;
            }, context.Response);

        }

        await _next.Invoke(context);
    }
}
