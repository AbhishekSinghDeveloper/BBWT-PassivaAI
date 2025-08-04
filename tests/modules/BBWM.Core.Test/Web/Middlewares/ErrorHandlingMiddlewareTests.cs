using BBWM.Core.Services;
using BBWM.Core.Web.Middlewares;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using System.ComponentModel.DataAnnotations;

using Xunit;

namespace BBWM.Core.Test.Web.Middlewares;

public class ErrorHandlingMiddlewareTests
{
    public ErrorHandlingMiddlewareTests()
    {
    }

    private static (ErrorHandlingMiddleware, Mock<IErrorNotifyService>) GetService()
    {
        HttpContext ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        RequestDelegate next = hc => throw new Exception("Generic error");

        var mockRequestDelegate = new Mock<RequestDelegate>();
        mockRequestDelegate.Setup(p => p.Invoke(ctx)).Returns(Task.FromResult(new ValidationException()));
        var mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        var mockErrorNotifyService = new Mock<IErrorNotifyService>();
        mockErrorNotifyService
            .Setup(e => e.NotifyOnException(It.IsAny<Exception>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        return (
            new ErrorHandlingMiddleware(
                next,
                mockLogger.Object,
                mockWebHostEnvironment.Object,
                mockErrorNotifyService.Object),
            mockErrorNotifyService);
    }

    [Fact]
    public async Task Should_Handle_Exception()
    {
        // Arrange
        var (service, errorNotifier) = GetService();
        var httpContext = new DefaultHttpContext();

        // Act
        await service.Invoke(httpContext);

        // Assert
        errorNotifier.Verify();
    }
}
