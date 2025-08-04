using AutoMapper;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Service;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Test.Utils;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

using Moq;

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

using Xunit;

using MyMappingFixture = BBWM.AWS.EventBridge.Test.Fixtures.MappingFixture;

namespace BBWM.AWS.EventBridge.Test.Service;

public class AwsEventBridgeRuleServiceTests : IClassFixture<MyMappingFixture>
{
    public AwsEventBridgeRuleServiceTests(MyMappingFixture mappingFixture)
        => Mapper = mappingFixture.Mapper;

    public IMapper Mapper { get; }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Save_ShouldFailIfJobNotRegistered(bool isCreate)
    {
        // Arrange
        var jobService = CreateJobService(false);
        var error = "The given target job doesn't exist.";
        var optionsSnapshot = GetAwsSettingsOptions();
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        // Act
        var sut = new AwsEventBridgeRuleService(
            Mapper, dataService, optionsSnapshot.Object, null, null, null, null, jobService.Object, null);

        var save = GetSaveHandler(isCreate, sut);

        // Assert
        var ex = await Assert.ThrowsAsync<ObjectNotExistsException>(
            () => save(
                new AwsEventBridgeRuleDTO { TargetJobId = "DummyJobId" },
                default));
        Assert.Equal(error, ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Save_ShouldThrowIfInvalidCron(bool isCreate)
    {
        // Arrange
        var jobService = CreateJobService(true);
        var jobInfo = new AwsEventBridgeJobInfoDTO
        { Parameters = new List<JobParameterInfo>() };
        jobService.Setup(j => j.GetJobInfo(It.IsAny<string>())).Returns(jobInfo);
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule_ResourceNotFound()
            .PutRule_BadRequest_ScheduleExpression()
            .BuildFactory();
        var options = GetAwsSettingsOptions();
        const string CRON = "* * :-) * ? *";
        var error = $"The expression \"{CRON}\" is not correct.";

        // Act
        var sut = new AwsEventBridgeRuleService(
           Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);

        var save = GetSaveHandler(isCreate, sut);

        // Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => save(
                    new AwsEventBridgeRuleDTO
                    {
                        Name = "DummyRuleId",
                        TargetJobId = "DummyTargetJobId",
                        Cron = CRON,
                    },
                    default));
        Assert.Equal(error, ex.Message);
    }

    [Theory]
    [MemberData(
        nameof(AwsEventBridgeRuleServiceTestData.ParameterTestData),
        MemberType = typeof(AwsEventBridgeRuleServiceTestData))]
    public async Task Save_ShouldThrowInvalidParameter(TestParameterInfo testParamInfo, bool isCreate)
    {
        // Arrange
        var jobService = CreateJobService(true);
        var jobInfo = new AwsEventBridgeJobInfoDTO
        { Parameters = testParamInfo.Parameters, };
        jobService.Setup(j => j.GetJobInfo(It.IsAny<string>())).Returns(jobInfo);
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule_ResourceNotFound()
            .BuildFactory();

        var options = GetAwsSettingsOptions();

        // Act
        var sut = new AwsEventBridgeRuleService(
           Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);
        var save = GetSaveHandler(isCreate, sut);

        // Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => save(
                    new AwsEventBridgeRuleDTO
                    {
                        Name = "DummyRuleId",
                        TargetJobId = "DummyTargetJobId",
                        Parameters = testParamInfo.InputParameters,
                    },
                    default));
        Assert.Equal(testParamInfo.Error, ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Save_ShouldThrowUnknownError(bool isCreate)
    {
        // Arrange
        var jobService = CreateJobService(true);
        var jobInfo = new AwsEventBridgeJobInfoDTO
        { Parameters = new List<JobParameterInfo>() };
        jobService.Setup(j => j.GetJobInfo(It.IsAny<string>())).Returns(jobInfo);
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule_ResourceNotFound()
            .PutRule_GenericError()
            .BuildFactory();
        var options = GetAwsSettingsOptions();
        const string JOB_ID = "DummyJobId";
        var error = $"An error has occurred while saving the job \"{JOB_ID}\".";

        // Act
        var sut = new AwsEventBridgeRuleService(
           Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);
        var save = GetSaveHandler(isCreate, sut);

        // Assert
        var ex = await Assert.ThrowsAsync<ApiException>(
            () => save(
                    new AwsEventBridgeRuleDTO { Name = JOB_ID },
                    default));
        Assert.Equal(error, ex.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Save_ShouldSaveJob(bool isCreate)
    {
        // Arrange
        var jobService = CreateJobService(true);
        var jobInfo = new AwsEventBridgeJobInfoDTO
        { Parameters = new List<JobParameterInfo>() };
        jobService.Setup(j => j.GetJobInfo(It.IsAny<string>())).Returns(jobInfo);
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule_ResourceNotFound()
            .PutRule()
            .DescribeApiDestination()
            .PutTargets()
            .BuildFactory();
        var options = GetAwsSettingsOptions();
        const string RULE_ID = "DummyRuleId";
        const string JOB_ID = "DummyJobId";

        // Act
        var sut = new AwsEventBridgeRuleService(
          Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);
        var save = GetSaveHandler(isCreate, sut);
        var dto = await save(new AwsEventBridgeRuleDTO { Name = RULE_ID, TargetJobId = JOB_ID }, default);

        // Assert
        jobService.Verify();
        Assert.NotNull(dto);
        Assert.Equal(RULE_ID, dto.Name);
        Assert.Equal(JOB_ID, dto.TargetJobId);
    }

    [Fact]
    public async Task Save_ShouldSaveFirstJob()
    {
        // Arrange
        var jobService = CreateJobService(true);
        var jobInfo = new AwsEventBridgeJobInfoDTO
        { Parameters = new List<JobParameterInfo>() };
        jobService.Setup(j => j.GetJobInfo(It.IsAny<string>())).Returns(jobInfo);
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        // IUrlHelperFactory, IUrlHelper, IActionContextAccessor, IWebHostEnvironment
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(h => h.Action(It.IsAny<UrlActionContext>()))
            .Returns("https://dummy.com/api/aws-event-bridge-job/start-job/*");
        var urlFactory = new Mock<IUrlHelperFactory>();
        urlFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);
        var actionContextAccessor = new Mock<IActionContextAccessor>();
        actionContextAccessor.Setup(c => c.ActionContext).Returns((ActionContext)null);
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Test");
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(c => c.HttpContext.Request.Scheme).Returns("https");

        // Client factory to go deep down until create connection
        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .PutRule()
            .DescribeRule_ResourceNotFound()
            .DescribeApiDestination_NotFound()
            .DescribeConnection_NotFound()
            .CreateConnection()
            .CreateApiDestination()
            .PutTargets()
            .BuildFactory();
        var options = GetAwsSettingsOptions();
        const string RULE_ID = "MyDummyRule";
        const string JOB_ID = "DummyJobId";

        // Act
        var sut = new AwsEventBridgeRuleService(
            Mapper,
            dataService,
            options.Object,
            urlFactory.Object,
            actionContextAccessor.Object,
            httpContextAccessor.Object,
            environment.Object,
            jobService.Object,
            clientFactory.Object);
        var dto = await sut.Create(new() { TargetJobId = JOB_ID, Name = RULE_ID }, default);

        // Assert
        jobService.Verify();
        Assert.NotNull(dto);
        Assert.Equal(RULE_ID, dto.Name);
        Assert.Equal(JOB_ID, dto.TargetJobId);
    }

    [Fact]
    public async Task GetPage_ShouldReturnExtendedJob()
    {
        // Arrange
        const string JOB_ID = "DummyJobId";
        const string CRON = "* * * * ? *";
        const string RULE_ID = "MyRule";
        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .ListRules(RULE_ID, $"cron({CRON})")
            .BuildFactory();
        var jobService = new Mock<IAwsEventBridgeJobService>();
        jobService
            .Setup(s => s.GetAllAsync(
                It.IsAny<Expression<Func<AwsEventBridgeJobDTO, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AwsEventBridgeJobDTO>
            {
                    new AwsEventBridgeJobDTO
                    {
                        Id = 1,
                        JobId = JOB_ID,
                        RuleId = RULE_ID,
                        LastExecutionTime = DateTime.UtcNow,
                        NextExecutionTime = DateTime.UtcNow.AddMinutes(1),
                        TimeZone = TimeZoneInfo.Utc.Id,
                    },
            });
        var options = GetAwsSettingsOptions();
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        // Act
        var sut = new AwsEventBridgeRuleService(
          Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);
        var page = await sut.GetPage(
            new QueryCommand
            {
                Filters = new List<FilterInfoBase>
                {
                        new StringFilter
                        {
                            MatchMode = StringFilterMatchMode.Equals,
                            Value = RULE_ID,
                            PropertyName = nameof(AwsEventBridgeRuleDTO.Name),
                        },
                },
            }, default);

        // Assert
        Assert.NotNull(page);
        var item = Assert.Single(page.Items);
        Assert.Equal(RULE_ID, item.Name);
        Assert.Equal(JOB_ID, item.TargetJobId);
        Assert.Equal(CRON, item.Cron);
        Assert.NotNull(item.LastExecutionTime);
        Assert.NotNull(item.NextExecutionTime);
        Assert.NotNull(item.TimeZoneId);
    }

    [Fact]
    public async Task Delete_ShouldDeleteJob()
    {
        // Arrange
        var jobService = CreateJobService(true);
        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .RemoveTargets()
            .DeleteRule()
            .BuildFactory();
        var options = GetAwsSettingsOptions();
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper);

        // Act
        var sut = new AwsEventBridgeRuleService(
          Mapper, dataService, options.Object, null, null, null, null, jobService.Object, clientFactory.Object);
        await sut.Delete("DummyJobId", default);

        // Assert
        jobService.Verify();
    }

    private static Mock<IOptionsSnapshot<AwsEventBridgeSettings>> GetAwsSettingsOptions()
    {
        var optionsSnapshot = new Mock<IOptionsSnapshot<AwsEventBridgeSettings>>();
        optionsSnapshot.Setup(o => o.Value).Returns(new AwsEventBridgeSettings());
        return optionsSnapshot;
    }

    private static Mock<IAwsEventBridgeJobService> CreateJobService(bool isJobRegistered)
    {
        var jobService = new Mock<IAwsEventBridgeJobService>();
        jobService.Setup(s => s.IsJobRegistered(It.IsAny<string>())).Returns(isJobRegistered);

        return jobService;
    }

    private static Func<AwsEventBridgeRuleDTO,
                        CancellationToken,
                        Task<AwsEventBridgeRuleDTO>>
        GetSaveHandler(bool isCreate, AwsEventBridgeRuleService sut)
            => isCreate
                ? sut.Create
                : sut.Update;
}
