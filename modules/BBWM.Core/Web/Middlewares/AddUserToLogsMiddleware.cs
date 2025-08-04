using BBWM.Core.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Security.Claims;

namespace BBWM.Core.Web.Middlewares;

public class AddUsernameToLogsMiddleware
{
    private readonly RequestDelegate _next;

    public AddUsernameToLogsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userIdentity = context.User.Identity;
        if (userIdentity.IsAuthenticated)
        {
            using (LogContext.PushProperty("Username", context.User.FindFirstValue(ClaimTypes.Name)))
            using (LogContext.PushProperty("UserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                await _next(context);
            }
        }
        else
        {
            using (LogContext.PushProperty("Username", "Anonymous"))
            {
                await _next(context);
            }
        }
    }
}

public class AddUserImpersonationToLogsMiddleware
{
    private readonly RequestDelegate _next;

    public AddUserImpersonationToLogsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userIdentity = context.User.Identity;
        if (userIdentity.IsAuthenticated)
        {
            var isImpersonating = context.User.HasClaim("IsImpersonating", "true");
            using (LogContext.PushProperty("IsImpersonating", isImpersonating))
            {
                if (isImpersonating)
                {
                    using (LogContext.PushProperty("OriginalUserName", context.User.FindFirst("OriginalUserName").Value))
                    using (LogContext.PushProperty("OriginalUserId", context.User.FindFirst("OriginalUserId").Value))
                    {
                        await _next(context);
                    }
                }
                else
                {
                    await _next(context);
                }
            }
        }
        else
        {
            await _next(context);
        }
    }
}

public class AddUserIpToLogsMiddleware
{
    private readonly RequestDelegate _next;

    public AddUserIpToLogsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var ip = context.GetUserIp();
        using (LogContext.PushProperty("IP", ip))
        {
            await _next(context);
        }
    }
}
