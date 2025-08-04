using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace BBWM.Core.Web.CookieAuth;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly ISettingsService _settingsService;
    private readonly CookieAuthSettings _cookieAuthConfig;

    public CustomCookieAuthenticationEvents(ISettingsService settingsService, IOptionsSnapshot<CookieAuthSettings> cookieAuthConfig)
    {
        _settingsService = settingsService;
        _cookieAuthConfig = cookieAuthConfig.Value;
        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        => Handler(StatusCodes.Status401Unauthorized, context);

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        => Handler(StatusCodes.Status403Forbidden, context);

    private Task Handler(int statusCode, RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Request.Path.StartsWithSegments(_cookieAuthConfig.ApiPath))
        {
            context.Response.Headers[HeaderNames.Location] = context.RedirectUri;
            context.Response.StatusCode = statusCode;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    }
}
