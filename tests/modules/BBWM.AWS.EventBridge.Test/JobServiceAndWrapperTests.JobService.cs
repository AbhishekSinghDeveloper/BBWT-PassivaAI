using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.AWS.EventBridge.Service;
using BBWM.AWS.EventBridge.Test.Jobs;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.Core.Test.Utils;
using BBWT.Tests.modules.BBWM.AWS.EventBridge.Test.Jobs;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BBWM.AWS.EventBridge.Test;

public partial class JobServiceAndWrapperTests
{
    [Fact]
    [TestPriority(1)]
    public void JobService_RegisterJob_JobShouldBeRegistered()
    {
        // Arrange
        var job = new TestEventBridgeJob();
        var scopeFactoryMock = CreateScopeFactoryMock(Mock.Of<IDataService>(), job, TestEventBridgeJob.Metadata);

        // Act
        var sut = CreateRegisterJobSUT(scopeFactoryMock);
        sut.RegisterJob<TestEventBridgeJob>();

        // Assert
        Assert.True(sut.IsJobRegistered(TestEventBridgeJob.JOB_ID));
    }

    [Fact]
    [TestPriority(2)]
    public void JobService_RegisterJob_ShouldFailWhenJobAlreadyRegistered()
    {
        // Arrange
        var job = new TestEventBridgeJob();
        var scopeFactoryMock = CreateScopeFactoryMock(Mock.Of<IDataService>(), job, TestEventBridgeJob.Metadata);

        // Act
        var sut = CreateRegisterJobSUT(scopeFactoryMock);

        // Assert
        Assert.Throws<AwsEventBridgeException>(() => sut.RegisterJob<TestEventBridgeJob>());
    }

    [Fact]
    [TestPriority(3)]
    public void JobService_IsJobRegistered_ShouldReturnTrue()
    {
        // Arrange

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, null);

        // Assert
        Assert.True(sut.IsJobRegistered(TestEventBridgeJob.JOB_ID));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("DummyJobId")]
    [TestPriority(4)]
    public void JobService_IsJobRegistered_ShouldReturnFalse(string jobId)
    {
        // Arrange

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, null);

        // Assert
        Assert.False(sut.IsJobRegistered(jobId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("DummyJobId")]
    [TestPriority(5)]
    public void JobService_GetJobInfo_ShouldReturnNull(string jobId)
    {
        // Arrange

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, null);

        // Assert
        Assert.Null(sut.GetJobInfo(jobId));
    }

    [Fact]
    [TestPriority(6)]
    public void JobService_GetJobInfo_ShouldReturnJobInfo()
    {
        // Arrange

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, null);
        var jobInfo = sut.GetJobInfo(TestEventBridgeJob.JOB_ID);

        // Assert
        Assert.NotNull(jobInfo);
        Assert.Equal(TestEventBridgeJob.JOB_ID, jobInfo.JobId);
        Assert.Equal(TestEventBridgeJob.JOB_DESCRIPTION, jobInfo.JobDescription);
    }

    [Fact]
    [TestPriority(7)]
    public void JobService_RegisterJob_ShouldFailIfParamsRepeated()
    {
        // Arrange
        var job = new InvalidParamsJob();
        var duplicateError = $"Job \"{InvalidParamsJob.JOB_ID}\" cannot repeat parameters.";
        var scopeFactoryMock = CreateScopeFactoryMock(Mock.Of<IDataService>(), job, InvalidParamsJob.Metadata);

        // Act
        var sut = CreateRegisterJobSUT(scopeFactoryMock);

        // Assert
        var ex = Assert.Throws<AwsEventBridgeException>(() => sut.RegisterJob<InvalidParamsJob>());
        Assert.Equal(duplicateError, ex.Message);
    }

    [Fact]
    [TestPriority(8)]
    public async Task JobService_GetJobsListAsync_ShouldHaveOneJobAvailable()
    {
        // Arrange

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, null);

        var jobs = await sut.GetJobsListAsync(default);

        var job = Assert.Single(jobs);
        Assert.Equal(TestEventBridgeJob.JOB_ID, job.JobId);
    }

    [Fact]
    [TestPriority(9)]
    public async Task JobService_StartJobAsync_ShouldThrowIfRuleNotFound()
    {
        // Arrange
        const string DUMMY_RULE_ID = "DummyRuleId";
        var error = $"Rule \"{DUMMY_RULE_ID}\" doesn't exist.";

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule_ResourceNotFound()
            .BuildFactory();

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), clientFactory.Object, null);
        var startInfo = new AwsEventBridgeStartJobDTO { RuleId = DUMMY_RULE_ID };

        // Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.StartJobAsync(startInfo));
        Assert.Equal(error, ex.Message);
    }

    [Fact]
    [TestPriority(10)]
    public async Task JobService_StartJobAsync_ShouldThrowIfJobNotFound()
    {
        // Arrange
        var clientFactory = AmazonEventBridgeMockHelper.CreateClient().DescribeRule().BuildFactory();

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), clientFactory.Object, null);
        var startInfo = new AwsEventBridgeStartJobDTO { RuleId = default };

        // Assert
        await Assert.ThrowsAsync<ApiException>(() => sut.StartJobAsync(startInfo));
    }

    [Fact]
    [TestPriority(11)]
    public async Task JobService_StartJobAsync_ShouldStartJob()
    {
        // Arrange
        var jobId = Guid.NewGuid().ToString("N");
        var job = new Mock<TestEventBridgeJob>().As<IEventBridgeJob>();

        job
            .Setup(j => j.RunAsync(
                It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var ruleId = "MyRuleId";

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule(ruleId)
            .BuildFactory();

        var dataService = await GetDataServiceWithJob(ruleId, jobId, default);
        var startInfo = new AwsEventBridgeStartJobDTO { RuleId = ruleId };
        var metadata = Activator.CreateInstance(typeof(MockMetadata<>).MakeGenericType(job.Object.GetType()), new object[] { jobId, null, null }) as IEventBridgeJobMetadata;
        var scopeFactory = CreateScopeFactoryMock(dataService, job.Object, metadata);
        var sut = new AwsEventBridgeJobService(Mapper, dataService, clientFactory.Object, scopeFactory);
        RegisterMockJob(sut, job.Object.GetType());

        // Act
        await sut.StartJobAsync(startInfo);
        await Task.Delay(100);

        // Assert
        job.Verify();
    }

    [Fact]
    [TestPriority(12)]
    public async Task JobService_StartJobAsync_ShouldThrowWhenRuleDisabled()
    {
        // Arrange
        var ruleId = "MyRuleId";
        var error = $"Rule \"{ruleId}\" is disabled.";
        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeDisabledRule(ruleId)
            .BuildFactory();

        var startInfo = new AwsEventBridgeStartJobDTO { RuleId = ruleId };

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), clientFactory.Object, null);

        // Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.StartJobAsync(startInfo));
        Assert.Equal(error, ex.Message);
    }

    [Fact]
    [TestPriority(13)]
    public async Task JobService_GetAllAsync_ShouldReturnAllJobs()
    {
        // Arrange
        const string rule1 = "MyRule1";
        const string rule2 = "MyRule2";
        var (dataService, rules) = await CreateGetAllAsyncDbContext(rule1, rule2);

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, dataService, null, null);
        var result = await sut.GetAllAsync(null, CancellationToken.None);

        // Assert
        AssertGetAllAsyncResult(2, rules, result);
    }

    [Fact]
    [TestPriority(14)]
    public async Task JobService_GetAllAsync_ShouldReturnFilteredJobs()
    {
        // Arrange
        const string rule1 = "MyRule1";
        const string rule2 = "MyRule2";
        var (dataService, rules) = await CreateGetAllAsyncDbContext(rule1, rule2);

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, dataService, null, null);
        var result = await sut.GetAllAsync(j => j.RuleId == rule1, CancellationToken.None);

        // Assert
        AssertGetAllAsyncResult(1, rules, result);
    }

    [Fact]
    [TestPriority(15)]
    public async Task JobService_RestartJobAsync_ShouldThrow()
    {
        // Arrange
        var dataService = new Mock<IDataService>();
        dataService
            .Setup(h => h.Get<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AwsEventBridgeJobHistoryDTO)null);

        // Act
        var sut = new AwsEventBridgeJobService(Mapper, dataService.Object, null, null);

        // Arrange
        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.RestartJobAsync(1, CancellationToken.None));
    }

    [Fact]
    [TestPriority(16)]
    public async Task JobService_RestartJobAsync_ShouldRestartJob()
    {
        // Arrange
        var jobId = Guid.NewGuid().ToString("N");
        var job = new Mock<TestEventBridgeJob>().As<IEventBridgeJob>();

        job
            .Setup(j => j.RunAsync(
                It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        const string RULE_ID = "MyRuleId";
        const int HISTORY_ID = 1;

        var clientFactory = AmazonEventBridgeMockHelper
            .CreateClient()
            .DescribeRule(RULE_ID)
            .BuildFactory();

        var dataService = await GetDataServiceWithJob(RULE_ID, jobId, default);
        await dataService.Create<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(
            new AwsEventBridgeJobHistoryDTO
            {
                Id = HISTORY_ID,
                RuleId = RULE_ID,
                JobId = jobId,
            },
            CancellationToken.None);
        var metadata = Activator.CreateInstance(typeof(MockMetadata<>).MakeGenericType(job.Object.GetType()), new object[] { jobId, null, null }) as IEventBridgeJobMetadata;
        var scopeFactory = CreateScopeFactoryMock(dataService, job.Object, metadata);
        var sut = new AwsEventBridgeJobService(Mapper, dataService, clientFactory.Object, scopeFactory);
        RegisterMockJob(sut, job.Object.GetType());

        // Act
        await sut.RestartJobAsync(HISTORY_ID, CancellationToken.None);
        await Task.Delay(100);

        // Assert
        job.Verify();
    }

    private static void AssertGetAllAsyncResult(
        int expectedCount, Dictionary<string, JobAssertInfo> rules, List<AwsEventBridgeJobDTO> result)
    {
        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count);

        foreach (var item in result)
        {
            Assert.True(rules.ContainsKey(item.RuleId));
            var assertInfo = rules[item.RuleId];

            Assert.Equal(assertInfo.JobId, item.JobId);
            Assert.Equal(assertInfo.Params.Count, item.Parameters?.Count ?? -1);

            var itemParams = item.Parameters ?? new List<AwsEventBridgeJobParameterDTO>();
            foreach (var assertParamInfo in assertInfo.Params)
            {
                Assert.Contains(
                    itemParams,
                    (p) => string.CompareOrdinal(p.Name, assertParamInfo.Name) == 0 &&
                           string.CompareOrdinal(p.Value, assertParamInfo.Value) == 0);
            }
        }
    }

    public async Task<IDataService> GetDataServiceWithJob(
        string ruleId,
        string jobId,
        List<AwsEventBridgeJobParameterDTO> @params,
        IDataService dataService = default,
        IDbContext dbContext = default)
    {
        dataService ??= SutDataHelper.CreateEmptyDataService(Mapper, ctx: dbContext);

        await dataService.Create<EventBridgeJob, AwsEventBridgeJobDTO>(
            new AwsEventBridgeJobDTO
            {
                RuleId = ruleId,
                JobId = jobId,
                Parameters = @params,
            },
            CancellationToken.None);

        return dataService;
    }

    private async Task<(IDataService, Dictionary<string, JobAssertInfo>)> CreateGetAllAsyncDbContext(
        string rule1, string rule2)
    {
        var rules = new[] { rule1, rule2 }.ToDictionary(
            r => r,
            r => new JobAssertInfo
            {
                RuleId = r,
                JobId = $"{r}_Job",
                Params = new List<AwsEventBridgeJobParameterDTO>
                {
                        new AwsEventBridgeJobParameterDTO { Name = $"{r}_P1", Value = $"{r}_V1" },
                        new AwsEventBridgeJobParameterDTO { Name = $"{r}_P2", Value = $"{r}_V2" },
                },
            });

        var dataService = await GetDataServiceWithJob(rule1, rules[rule1].JobId, rules[rule1].Params);

        return (
            await GetDataServiceWithJob(rule2, rules[rule2].JobId, rules[rule2].Params, dataService),
            rules);
    }

    private IServiceScopeFactory CreateScopeFactoryMock(IDataService dataService, IEventBridgeJob job, IEventBridgeJobMetadata metadata)
    {
        var scope = CreateServiceScope(dataService, job, metadata);

        return scope.ServiceProvider.GetService(typeof(IServiceScopeFactory)) as IServiceScopeFactory;
    }

    private static void RegisterMockJob(AwsEventBridgeJobService service, Type jobType)
    {
        var m = typeof(AwsEventBridgeJobService).GetMethod(nameof(AwsEventBridgeJobService.RegisterJob)).MakeGenericMethod(jobType);
        m.Invoke(service, Array.Empty<object>());
    }

    private AwsEventBridgeJobService CreateRegisterJobSUT(IServiceScopeFactory scopeFactoryMock)
        => new(Mapper, SutDataHelper.CreateEmptyDataService(Mapper), null, scopeFactoryMock);

    private class JobAssertInfo
    {
        public string RuleId { get; set; }

        public string JobId { get; set; }

        public List<AwsEventBridgeJobParameterDTO> Params { get; set; }
    }
}
