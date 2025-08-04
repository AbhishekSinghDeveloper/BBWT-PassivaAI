using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace BBWM.Core.Web.Middlewares;

public class HttpToHttpsRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpToHttpsRedirectMiddleware> _logger;

    public HttpToHttpsRedirectMiddleware(RequestDelegate next, ILogger<HttpToHttpsRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var lbClient = "";
        var lbProto = "";
        var lbPort = "";

        if (!string.IsNullOrEmpty(context.Request.Headers["X-Forwarded-For"]))
        {
            lbClient = context.Request.Headers["X-Forwarded-For"];
            lbProto = context.Request.Headers["X-Forwarded-Proto"];
            lbPort = context.Request.Headers["X-Forwarded-Port"];
        }

        if (!string.IsNullOrEmpty(lbClient) && lbPort == "80" && lbProto == "http")
        {
            context.Request.Scheme = "https";
            _logger.LogDebug($"[{lbClient}] Redirected to {context.Request.GetEncodedUrl()}");
            context.Response.Redirect(context.Request.GetEncodedUrl());
        }
        else
        {
            await _next.Invoke(context);
        }
    }
}
