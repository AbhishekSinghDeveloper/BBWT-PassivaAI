using Microsoft.AspNetCore.Builder;

using Moq;

using Xunit;

namespace BBWM.Core.Web.CookieAuth.Test;

public class MiddlewareExtensionsTests
{
    public MiddlewareExtensionsTests()
    {
    }

    [Fact]
    public void Use_Disable_Sliding_Expiration_Middleware_Test()
    {
        // Arrange
        var mock = new Mock<IApplicationBuilder>();

        var middlewareExtensions = MiddlewareExtensions.UseDisableSlidingExpirationMiddleware(mock.Object);

        Assert.Null(middlewareExtensions);
    }
}
