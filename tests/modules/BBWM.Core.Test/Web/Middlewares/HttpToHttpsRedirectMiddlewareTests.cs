using BBWM.Core.Web.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace BBWM.Core.Test.Web.Middlewares;

public class HttpToHttpsRedirectMiddlewareTests
{
    public HttpToHttpsRedirectMiddlewareTests()
    {
    }

    private static HttpToHttpsRedirectMiddleware GetService()
    {
        RequestDelegate next = (HttpContext hc) => Task.CompletedTask;
        var loggerMock = new Mock<ILogger<HttpToHttpsRedirectMiddleware>>();

        return new HttpToHttpsRedirectMiddleware(next, loggerMock.Object);
    }

    [Fact]
    public async Task Should_Redirect_to_Https()
    {
        // Arrange
        RequestDelegate next = hc => throw new Exception("Shouldn't get here");
        var service = new HttpToHttpsRedirectMiddleware(next, Mock.Of<ILogger<HttpToHttpsRedirectMiddleware>>());

        HttpContext ctx = new DefaultHttpContext();
        ctx.Request.Headers["X-Forwarded-For"] = "test";
        ctx.Request.Headers["X-Forwarded-Port"] = "80";
        ctx.Request.Headers["X-Forwarded-Proto"] = "http";

        // Act
        var error = await Record.ExceptionAsync(() => service.Invoke(ctx));

        // Assert
        Assert.Null(error);
        Assert.Equal("https", ctx.Request.Scheme);
        Assert.Equal(302, ctx.Response.StatusCode);
    }
}
