using BBWM.AWS.EventBridge.Api;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Moq;

using System.Net;

using Xunit;

namespace BBWM.AWS.EventBridge.Test.Api;

public class AwsEventBridgeJobStarterControllerTests
{
    [Fact]
    public async Task StartJob_ShouldReturnNoContent()
    {
        // Arrange
        var jobService = new Mock<IAwsEventBridgeJobService>();
        jobService
            .Setup(j => j.StartJobAsync(It.IsAny<AwsEventBridgeStartJobDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var sut = new AwsEventBridgeJobStarterController(jobService.Object);
        var actionsResult = await sut.StartJobAsync(default);

        // Assert
        jobService.Verify();
        var result = Assert.IsType<NoContentResult>(actionsResult);
        Assert.Equal((int)HttpStatusCode.NoContent, result.StatusCode);
    }
}
