using BBWM.AWS.EventBridge.Api;
using BBWM.AWS.EventBridge.Interfaces;

using Microsoft.AspNetCore.Mvc;

using Moq;

using System.Net;

using Xunit;

namespace BBWM.AWS.EventBridge.Test.Api;

public class AwsEventBridgeRuleControllerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RuleExists_ShouldReturnOk(bool ruleExist)
    {
        // Arrange
        var jobService = new Mock<IAwsEventBridgeRuleService>();
        jobService
            .Setup(j => j.RuleExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ruleExist)
            .Verifiable();

        // Act
        var sut = new AwsEventBridgeRuleController(jobService.Object);
        var actionsResult = await sut.RuleExistsAsync("MyRule", CancellationToken.None);

        // Assert
        jobService.Verify();
        var result = Assert.IsType<OkObjectResult>(actionsResult);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        var exists = Assert.IsType<bool>(result.Value);
        Assert.Equal(ruleExist, exists);
    }
}
