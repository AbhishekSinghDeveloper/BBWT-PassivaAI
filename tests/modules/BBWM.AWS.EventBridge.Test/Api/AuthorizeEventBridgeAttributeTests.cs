using BBWM.AWS.EventBridge.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.AWS.EventBridge.Test.Api;

public class AuthorizeEventBridgeAttributeTests
{
    private const string API_KEY = "zFDJldfJaRFrUbRiyetWsA==";
    private const string AUTH_HEADER = "X-Api-Key";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("zFDJldfJaRFrUbRiyetWsA==,,,,")]
    public void OnAuthorization_ShouldNotAuthorize(string headerValue)
    {
        // Arrange
        var authContext = CreateAuthContext(headerValue);

        // Act
        var sut = new AuthorizeEventBridgeAttribute();
        sut.OnAuthorization(authContext);

        // Assert
        Assert.NotNull(authContext.Result);
        Assert.IsType<UnauthorizedResult>(authContext.Result);
    }

    [Fact]
    public void OnAuthorization_ShouldAuthorize()
    {
        // Arrange
        var authContext = CreateAuthContext(API_KEY);

        // Act
        var sut = new AuthorizeEventBridgeAttribute();
        sut.OnAuthorization(authContext);

        // Assert
        Assert.Null(authContext.Result);
    }

    private static AuthorizationFilterContext CreateAuthContext(string apiKey)
    {
        var options = new Mock<IOptionsSnapshot<AwsEventBridgeSettings>>();
        options.Setup(o => o.Value).Returns(new AwsEventBridgeSettings
        {
            APIKey = API_KEY,
            AuthHeader = AUTH_HEADER,
        });

        var services = new Mock<IServiceProvider>();
        services
            .Setup(s => s.GetService(typeof(IOptionsSnapshot<AwsEventBridgeSettings>)))
            .Returns(options.Object);

        var headers = new HeaderDictionary();
        if (apiKey is not null)
        {
            headers[AUTH_HEADER] = apiKey;
        }

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.RequestServices).Returns(services.Object);
        httpContext.Setup(c => c.Request.Headers).Returns(headers);

        var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
        var authContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

        return authContext;
    }
}
