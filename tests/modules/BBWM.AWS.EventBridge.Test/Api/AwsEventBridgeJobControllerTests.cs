using BBWM.AWS.EventBridge.Api;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Moq;

using System.Net;

using Xunit;

namespace BBWM.AWS.EventBridge.Test.Api;

public class AwsEventBridgeJobControllerTests
{
    [Fact]
    public async Task RestartJobAsync_ShouldRestartJob()
    {
        // Arrange
        var jobService = new Mock<IAwsEventBridgeJobService>();
        jobService
            .Setup(j => j.RestartJobAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        var sut = new AwsEventBridgeJobController(jobService.Object);
        var actionResult = await sut.RestartJobAsync(1);

        // Assert
        jobService.Verify();
        var result = Assert.IsType<NoContentResult>(actionResult);
        Assert.Equal((int)HttpStatusCode.NoContent, result.StatusCode);
    }

    [Fact]
    public async Task GetJobsList_ShouldReturnOk()
    {
        // Arrange
        const string JOB_ID = "DummyJobId";

        var jobService = new Mock<IAwsEventBridgeJobService>();
        jobService
            .Setup(j => j.GetJobsListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AwsEventBridgeJobInfoDTO>
            {
                    new AwsEventBridgeJobInfoDTO { JobId = JOB_ID },
            })
            .Verifiable();

        // Act
        var sut = new AwsEventBridgeJobController(jobService.Object);
        var actionsResult = await sut.GetJobsListAsync(default);

        // Assert
        jobService.Verify();
        var result = Assert.IsType<OkObjectResult>(actionsResult);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        var list = Assert.IsType<List<AwsEventBridgeJobInfoDTO>>(result.Value);
        var item = Assert.Single(list);
        Assert.Equal(JOB_ID, item.JobId);
    }
}
