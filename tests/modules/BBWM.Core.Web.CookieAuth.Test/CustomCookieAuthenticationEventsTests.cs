using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Moq;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class CustomCookieAuthenticationEventsTests
{
    private const string RedirectUri = "/app/redirect/uri";
    private const string ApiPath = "/api";

    [Theory]
    [InlineData("/api/organization/1", StatusCodes.Status401Unauthorized)]
    [InlineData("/v2/api/organization/1", StatusCodes.Status302Found)]
    public async Task RedirectToLogin(string requestPath, int expectedStatusCode)
    {
        // Arrange
        var (authenticationEvents, context) = CreateService(requestPath);

        // Act
        await authenticationEvents.RedirectToLogin(context);

        // Assert
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.Equal(RedirectUri, context.Response.Headers[HeaderNames.Location]);
    }

    [Theory]
    [InlineData("/api/organization/1", StatusCodes.Status403Forbidden)]
    [InlineData("/v2/api/organization/1", StatusCodes.Status302Found)]
    public async Task RedirectToAccessDenied(string requestPath, int expectedStatusCode)
    {
        // Arrange
        var (authenticationEvents, context) = CreateService(requestPath);

        // Act
        await authenticationEvents.RedirectToAccessDenied(context);

        // Assert
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.Equal(RedirectUri, context.Response.Headers[HeaderNames.Location]);
    }

    private static (CustomCookieAuthenticationEvents, RedirectContext<CookieAuthenticationOptions>) CreateService(string requestPath)
    {
        var cookieAuthSettings = new Mock<IOptionsSnapshot<CookieAuthSettings>>();
        cookieAuthSettings.Setup(o => o.Value).Returns(new CookieAuthSettings { ApiPath = ApiPath });

        HttpContext httpContext = new DefaultHttpContext();
        httpContext.Request.Path = requestPath;

        var auth = new AuthenticationScheme("test", "test", Mock.Of<IAuthenticationHandler>().GetType());
        var authenticationProperties = new AuthenticationProperties();


        RedirectContext<CookieAuthenticationOptions> context =
            new(httpContext, auth, Mock.Of<CookieAuthenticationOptions>(), authenticationProperties, RedirectUri);
        CustomCookieAuthenticationEvents customCookieAuthenticationEvents =
            new(Mock.Of<ISettingsService>(), cookieAuthSettings.Object);

        return (customCookieAuthenticationEvents, context);
    }
}
