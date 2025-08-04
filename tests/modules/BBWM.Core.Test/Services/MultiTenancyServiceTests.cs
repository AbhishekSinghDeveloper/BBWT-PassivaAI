using BBWM.Core.Services;

using Microsoft.AspNetCore.Http;

using Moq;

using System.Security.Principal;

using Xunit;

namespace BBWM.Core.Test.Services;

public class MultiTenancyServiceTests
{
    public MultiTenancyServiceTests()
    {
    }

    private static MultiTenancyService GetService()
    {
        var identity = new Mock<IIdentity>();
        identity.Setup(p => p.IsAuthenticated).Returns(true);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        var context = new DefaultHttpContext();
        mockHttpContextAccessor.Setup(p => p.HttpContext).Returns(context);

        return new MultiTenancyService(
            mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetTenancyId_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = GetService();

        // Act
        var result = service.GetTenancyId();

        // Assert
        Assert.Null(result);
    }

    private class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
