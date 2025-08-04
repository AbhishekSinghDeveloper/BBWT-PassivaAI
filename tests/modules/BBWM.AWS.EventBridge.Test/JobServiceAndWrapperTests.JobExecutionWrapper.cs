using Autofac;
using Autofac.Extras.Moq;
using Autofac.Features.OwnedInstances;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.AWS.EventBridge.Service;
using BBWM.AWS.EventBridge.Test.Jobs;
using BBWM.Core.Services;
using BBWM.Core.Test.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace BBWM.AWS.EventBridge.Test;

public partial class JobServiceAndWrapperTests
{
    private const string RULE_ID = "MyRuleId";

    [Fact]
    [TestPriority]
    public async Task JobExecutionWrapper_JobShouldBeMarkedAsRunning()
    {
        // Arrange
        var (dataService, bkpDataService) = await GetDataServiceBkpAsync(
            nameof(JobExecutionWrapper_JobShouldBeMarkedAsRunning));
        var isRunning = false;
        var job = new TestExecutionJob(false);
        job.OnStart += async (ct) =>
        {
            var running = await bkpDataService.Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                    q => q.Where(dto => dto.RuleId == RULE_ID), ct);
            isRunning = running is not null;
        };
        var scope = CreateServiceScope(dataService, job, TestExecutionJob.Metadata);

        // Act
        JobExecutionWrapper.TrackJobExecution(scope, RULE_ID, typeof(TestExecutionJob), new());

        // Assert
        AssertTest(() => Assert.True(isRunning), job);
    }

    [Fact]
    [TestPriority]
    public async Task JobExecutionWrapper_JobShouldBeMarkedAsNotRunning()
    {
        // Arrange
        var (dataService, bkpDataService) = await GetDataServiceBkpAsync(
            nameof(JobExecutionWrapper_JobShouldBeMarkedAsNotRunning));
        var job = new TestExecutionJob(false);
        var scope = CreateServiceScope(dataService, job, TestExecutionJob.Metadata);

        // Act
        JobExecutionWrapper.TrackJobExecution(scope, RULE_ID, typeof(TestExecutionJob), new());

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 await Task.Delay(300);

                 var running = await bkpDataService.Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                     q => q.Where(j => j.RuleId == RULE_ID));
                 Assert.Null(running);
             },
             job);
    }

    [Fact]
    [TestPriority]
    public async Task JobExecutionWrapper_ShouldSetLastExecutionTime()
    {
        // Arrange
        var (dataService, bkpDataService) = await GetDataServiceBkpAsync(
            nameof(JobExecutionWrapper_ShouldSetLastExecutionTime));
        var job = new TestExecutionJob(false);
        var ourJobPrevLastExecution = (await dataService
            .Get<EventBridgeJob, AwsEventBridgeJobDTO>(
                q => q.Where(j => j.RuleId == RULE_ID), CancellationToken.None))
            .LastExecutionTime;
        var scope = CreateServiceScope(dataService, job, TestExecutionJob.Metadata);

        // Act
        JobExecutionWrapper.TrackJobExecution(scope, RULE_ID, typeof(TestExecutionJob), new());

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 Assert.Null(ourJobPrevLastExecution);
                 await Task.Delay(300);

                 var ourJob = await bkpDataService.Get<EventBridgeJob, AwsEventBridgeJobDTO>(
                     q => q.Where(j => j.RuleId == RULE_ID));
                 Assert.NotNull(ourJob?.LastExecutionTime);
             },
             job);
    }

    [Fact]
    [TestPriority]
    public async Task JobExecutionWrapper_ShouldSetNextExecution()
    {
        // Arrange
        var (dataService, bkpDataService) = await GetDataServiceBkpAsync(
            nameof(JobExecutionWrapper_ShouldSetNextExecution));

        var job = new TestExecutionJob(true);
        var ourJobPrevNextExecution = (await dataService
            .Get<EventBridgeJob, AwsEventBridgeJobDTO>(
                q => q.Where(j => j.RuleId == RULE_ID), CancellationToken.None))
            .NextExecutionTime;
        var scope = CreateServiceScope(dataService, job, TestExecutionJob.Metadata);

        // Act
        JobExecutionWrapper.TrackJobExecution(scope, RULE_ID, typeof(TestExecutionJob), new());

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 Assert.Null(ourJobPrevNextExecution);
                 await Task.Delay(300);

                 var ourJob = await bkpDataService.Get<EventBridgeJob, AwsEventBridgeJobDTO>(
                     q => q.Where(j => j.RuleId == RULE_ID));
                 Assert.NotNull(ourJob?.NextExecutionTime);
             },
             job);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [TestPriority]
    public async Task JobExecutionWrapper_JobShouldBeTracked(bool shouldFail, bool handlerShouldThrow)
    {
        // Arrange
        var (dataService, dataServiceBkp) = await GetDataServiceBkpAsync(
            nameof(JobExecutionWrapper_JobShouldBeTracked));
        var job = new TestExecutionJob(shouldFail);
        var errorHandler = CreateErrorHandler(false, shouldFail, failShouldThrow: handlerShouldThrow);
        var scope = CreateServiceScope(dataService, job, TestExecutionJob.Metadata, errorHandler: errorHandler.Object);

        // Act
        JobExecutionWrapper.TrackJobExecution(scope, RULE_ID, typeof(TestExecutionJob), new());

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 await Task.Delay(300);

                 var history = Assert.Single(await dataServiceBkp.GetAll<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>());
                 var completionStatus = shouldFail
                    ? JobCompletionStatus.Failed
                    : JobCompletionStatus.Succeed;

                 Assert.Equal(completionStatus, history.CompletionStatus);

                 errorHandler.Verify();
             },
             job);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [TestPriority]
    public async Task JobExecutionWrapper_JobShouldBeCanceledByUser(bool handlerShouldThrow)
    {
        // Arrange
        var jobInfo = await CreateContextForJobCancelation(handlerShouldThrow);

        // Act
        JobExecutionWrapper.TrackJobExecution(jobInfo.Scope, RULE_ID, jobInfo.JobType, jobInfo.Parameters);
        await Task.Delay(500); // Give enough time so the job can start running
        IDataService dataService = jobInfo.DataService;

        var cancelationId = (await dataService.Context.Set<EventBridgeRunningJob>().FirstOrDefaultAsync()).CancelationId;
        JobExecutionWrapper.CancelByUser(cancelationId);

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 await Task.Delay(300);

                 var history = Assert.Single(await dataService.GetAll<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>());
                 Assert.Equal(JobCompletionStatus.CanceledByUser, history.CompletionStatus);

                 jobInfo.ErrorHandler.Verify();
             },
             jobInfo.Job);
    }

    [Fact]
    [TestPriority(int.MaxValue)]
    public async Task JobExecutionWrapper_ShouldShutdown()
    {
        // Arrange
        var job1Info = await CreateContextForJobCancelation(false);
        var job2Info = await CreateContextForJobCancelation(false);

        // Act
        JobExecutionWrapper.TrackJobExecution(job1Info.Scope, job1Info.RuleId, job1Info.JobType, job1Info.Parameters);
        await Task.Delay(500); // Give enough time so the job can start running
        JobExecutionWrapper.CancelByShutdown();

        JobExecutionWrapper.TrackJobExecution(job2Info.Scope, job2Info.RuleId, job2Info.JobType, job2Info.Parameters);
        await Task.Delay(500); // Give enough time so the job can start running or not :-)

        var dataService1 = job1Info.DataService;
        var dataService2 = job2Info.DataService;

        // Assert
        await AssertTestAsync(
             async () =>
             {
                 await Task.Delay(300);

                 var history = Assert.Single(await dataService1.GetAll<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>());
                 Assert.Equal(JobCompletionStatus.CanceledByShutdown, history.CompletionStatus);

                 Assert.Empty(await dataService2.GetAll<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>());

                 job1Info.ErrorHandler.Verify();
             },
             job1Info.Job);
    }

    private async Task<TestJobInfo> CreateContextForJobCancelation(bool handlerShouldThrow, bool verifyCancel = true)
    {
        var job = new UntilCanceledJob();
        var errorHandler = CreateErrorHandler(verifyCancel: verifyCancel, verifyFail: false, cancelShouldThrow: handlerShouldThrow);

        var dbJob = new EventBridgeJob { RuleId = RULE_ID, JobId = UntilCanceledJob.JOB_ID, };
        var dataService =
            await SutDataHelper.CreateDataServiceWithData(Mapper, new[] { dbJob });
        var scope = CreateServiceScope(dataService, job, UntilCanceledJob.Metadata, errorHandler: errorHandler.Object);

        return new()
        {
            Job = job,
            JobType = typeof(UntilCanceledJob),
            Parameters = new(),
            RuleId = RULE_ID,
            Scope = scope,
            DataService = dataService,
            ErrorHandler = errorHandler,
        };
    }

    private IServiceScope CreateServiceScope(
        IDataService dataService, IEventBridgeJob job, IEventBridgeJobMetadata metadata, IAwsEventBridgeJobService jobService = null, IEventBridgeJobErrorHandler errorHandler = null)
    {
        var metadataType = typeof(IEventBridgeJobMetadata<>).MakeGenericType(job.GetType());
        var serviceScope = new Mock<IServiceScope>();
        var scopeFactory = new Mock<IServiceScopeFactory>();

        scopeFactory.Setup(f => f.CreateScope()).Returns(serviceScope.Object);

        serviceScope.Setup(s => s.ServiceProvider.GetService(job.GetType())).Returns(job);
        serviceScope.Setup(s => s.ServiceProvider.GetService(metadataType)).Returns(metadata);
        serviceScope.Setup(s => s.ServiceProvider.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory.Object);

        var clientFactory = AmazonEventBridgeMockHelper.CreateClient().DescribeRule(RULE_ID).BuildFactory();
        jobService ??= new AwsEventBridgeJobService(Mapper, dataService, clientFactory.Object, null);

        var mock = AutoMock.GetLoose(c =>
        {
            c.RegisterInstance(job).As(job.GetType());
            c.RegisterInstance(metadata).As(metadataType);
        });
        var lifetimeScope = mock.Create<ILifetimeScope>();

        var disposable = Mock.Of<IDisposable>();
        TrackingJobContext.Factory contextFactory =
            (jt, p) => new Owned<TrackingJobContext>(
                new(job.GetType(), new(), lifetimeScope, clientFactory.Object, jobService, dataService, errorHandler, Mock.Of<ILogger<IJobExecutionWrapper>>()),
                disposable);

        serviceScope
            .Setup(s => s.ServiceProvider.GetService(typeof(TrackingJobContext.Factory)))
            .Returns<Type>((t) => contextFactory);

        return serviceScope.Object;
    }

    private static async Task AssertTestAsync(Func<Task> asyncAsserts, ITestJob job)
    {
        WaitJobToFinish(job);

        if (job.Finished)
        {
            await asyncAsserts();
        }
        else
        {
            Assert.True(false, "Job didn't finished in two seconds.");
        }
    }

    private static Mock<IEventBridgeJobErrorHandler> CreateErrorHandler(
        bool verifyCancel = true,
        bool verifyFail = true,
        bool cancelShouldThrow = false,
        bool failShouldThrow = false)
    {
        var handler = new Mock<IEventBridgeJobErrorHandler>();

        var cancelSetup = cancelShouldThrow
            ? handler
                .Setup(h => h.HandleJobCancelation(
                    It.IsAny<string>(),
                    It.IsAny<IEventBridgeJob>(),
                    It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                    It.IsAny<bool>()))
                .Throws<Exception>()
            : handler
                .Setup(h => h.HandleJobCancelation(
                    It.IsAny<string>(),
                    It.IsAny<IEventBridgeJob>(),
                    It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                    It.IsAny<bool>()));

        var failSetup = failShouldThrow
            ? handler
                .Setup(h => h.HandleJobFailure(
                    It.IsAny<string>(),
                    It.IsAny<IEventBridgeJob>(),
                    It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                    It.IsAny<Exception>()))
                .Throws<Exception>()
            : handler.Setup(h => h.HandleJobFailure(
                It.IsAny<string>(),
                It.IsAny<IEventBridgeJob>(),
                It.IsAny<IEnumerable<AwsEventBridgeJobParameterDTO>>(),
                It.IsAny<Exception>()));

        if (verifyCancel)
        {
            cancelSetup.Verifiable();
        }

        if (verifyFail)
        {
            failSetup.Verifiable();
        }

        return handler;
    }

    private static void AssertTest(Action asserts, ITestJob job)
    {
        WaitJobToFinish(job);

        if (job.Finished)
        {
            asserts();
        }
        else
        {
            Assert.True(false, "Job didn't finished in two seconds.");
        }
    }

    private static void WaitJobToFinish(ITestJob job)
    {
        var start = DateTime.UtcNow;
        var maxWait = TimeSpan.FromSeconds(2);

        while (!job.Finished && DateTime.UtcNow - start < maxWait)
        {
            Thread.Sleep(100);
        }
    }

    private async Task<IDataService> GetDataServiceAsync(
        AwsEventBridgeJobDTO jobDto = default,
        DbType dbType = default,
        string dbName = default,
        bool insertJob = true)
    {
        var dataService = SutDataHelper.CreateEmptyDataService(Mapper, dbType, dbName);

        if (insertJob)
        {
            jobDto ??= new AwsEventBridgeJobDTO
            {
                RuleId = RULE_ID,
                JobId = TestExecutionJob.JOB_ID,
                TimeZone = TimeZoneInfo.Utc.Id,
            };
            await SutDataHelper.InsertData(dataService.Context, Mapper.Map<EventBridgeJob>(jobDto));
        }

        return dataService;
    }

    private async Task<(IDataService, IDataService)> GetDataServiceBkpAsync(
      string dbName,
      AwsEventBridgeJobDTO job = default)
    {
        var perCallDbName = $"{dbName}-{Guid.NewGuid():N}";

        var dataService = await GetDataServiceAsync(job, dbName: perCallDbName);
        var bkpDataService = await GetDataServiceAsync(dbName: perCallDbName, insertJob: false);

        return (dataService, bkpDataService);
    }

    private class TestJobInfo
    {
        public ITestJob Job { get; set; }

        public IServiceScope Scope { get; set; }

        public string RuleId { get; set; }

        public Type JobType { get; set; }

        public List<AwsEventBridgeJobParameterDTO> Parameters { get; set; }

        public IDataService DataService { get; set; }

        public Mock<IEventBridgeJobErrorHandler> ErrorHandler { get; set; }
    }
}
