using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Moq;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class DisableSlidingExpirationMiddlewareOptionsTests
{
    [Fact]
    public async Task Should_Disable_Sliding_Expiration()
    {
        // Arrange
        const string OffendingCookie = "Offending=Cookie";
        const string CoolCookie = "Cool=Cookie";

        RequestDelegate next = hc => Task.CompletedTask;
        var options = new Mock<IOptionsSnapshot<CookieAuthSettings>>();
        options.Setup(o => o.Value).Returns(new CookieAuthSettings { CookieName = "Offending" });

        var middleware = new DisableSlidingExpirationMiddleware(next, options.Object);

        Func<object, Task> onStarting = null;
        var responseHeaders = new HeaderDictionary()
        {
            [HeaderNames.SetCookie] = new[] { OffendingCookie, CoolCookie },
        };

        var httpResponse = new Mock<HttpResponse>();
        httpResponse
            .Setup(r => r.OnStarting(It.IsAny<Func<object, Task>>(), It.IsAny<object>()))
            .Callback<Func<object, Task>, object>((onStartingHandler, _) => onStarting = onStartingHandler);
        httpResponse.SetupGet(r => r.Headers).Returns(responseHeaders);

        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(c => c.Response).Returns(httpResponse.Object);
        httpContext.Setup(c => c.Items.ContainsKey(It.IsAny<object>())).Returns(true);

        // Act
        await middleware.Invoke(httpContext.Object);
        await onStarting(httpResponse.Object);

        // Assert
        var header = Assert.Single(responseHeaders);
        var cookie = Assert.Single(header.Value);
        Assert.Equal(CoolCookie, cookie);
    }
}
