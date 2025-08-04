using AutoMapper;

using BBWM.AWS.EventBridge.Api;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Services;

using BBWM.Core.Test.Utils;

using Bogus;

using Microsoft.AspNetCore.Mvc;

using System.Net;

using Xunit;

using MyMappingFixture = BBWM.AWS.EventBridge.Test.Fixtures.MappingFixture;

namespace BBWM.AWS.EventBridge.Test.Api;

public class AwsEventBridgeJobHistoryControllerTests : IClassFixture<MyMappingFixture>
{
    private static readonly JobCompletionStatus[] statuses
        = Enum.GetValues(typeof(JobCompletionStatus)).Cast<JobCompletionStatus>().ToArray();

    public AwsEventBridgeJobHistoryControllerTests(MyMappingFixture mappingFixture)
        => Mapper = mappingFixture.Mapper;

    public IMapper Mapper { get; }

    [Fact]
    public async Task GetCanceledJobsPage_Should_Return_Ok()
    {
        // Arrange
        var (sut, dataService, data) = await CreateSut();
        var total = data
            .Where(d => d.CompletionStatus == JobCompletionStatus.CanceledByShutdown ||
                        d.CompletionStatus == JobCompletionStatus.CanceledByUser)
            .Count();

        // Act
        var actionResult = await sut.GetCanceledJobsPageAsync(new(), dataService, CancellationToken.None);

        // Assert
        AssertActionResult(actionResult, total);
    }

    [Fact]
    public async Task GetPage_Should_Return_OK()
    {
        // Arrange
        var (sut, dataService, data) = await CreateSut();

        // Act
        var actionResult = await sut.GetPage(new(), dataService, CancellationToken.None);

        // Assert
        AssertActionResult(actionResult, data.Length);
    }

    private async Task<(AwsEventBridgeJobHistoryContoller, DataService, EventBridgeJobHistory[])> CreateSut()
    {
        var data = new Faker<EventBridgeJobHistory>()
            .RuleFor(h => h.JobId, f => f.Random.AlphaNumeric(5))
            .RuleFor(h => h.RuleId, f => f.Random.AlphaNumeric(7))
            .RuleFor(h => h.CompletionStatus, f => f.Random.ArrayElement(statuses))
            .Generate(100)
            .ToArray();

        var ctx = await SutDataHelper.CreateContextWithData(data);

        var dataService = new DataService(ctx, Mapper);
        return (new AwsEventBridgeJobHistoryContoller(), dataService, data);
    }

    private static void AssertActionResult(IActionResult actionResult, int total)
    {
        var result = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        var page = Assert.IsType<PageResult<AwsEventBridgeJobHistoryDTO>>(result.Value);
        Assert.Equal(total, page.Total);
    }
}
