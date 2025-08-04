using AutoMapper;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Data;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Services;

using BBWM.Core.Test.Utils;

using Bogus;

using Microsoft.EntityFrameworkCore;

using Moq;

using System.Diagnostics.CodeAnalysis;

using Xunit;

using EbMappingFixture = BBWM.AWS.EventBridge.Test.Fixtures.MappingFixture;

namespace BBWM.Core.Test.Services;

public class DataServiceTests : IClassFixture<EbMappingFixture>, IEqualityComparer<EventBridgeRunningJob>
{
    private static readonly Randomizer randomizer = new Randomizer();
    private const string JOBID_PREFIX = "_Common";

    public IMapper Mapper { get; }

    public DataServiceTests(EbMappingFixture mappingFixture)
    {
        Mapper = mappingFixture.Mapper;
    }

    public bool Equals([AllowNull] EventBridgeRunningJob x, [AllowNull] EventBridgeRunningJob y)
        => x?.Id == y?.Id && x?.JobId == y?.JobId && x?.RuleId == y?.RuleId;

    public int GetHashCode([DisallowNull] EventBridgeRunningJob obj)
        => $"{obj?.Id}::{obj?.JobId}::{obj?.RuleId}".GetHashCode();

    [Fact]
    public async Task Should_CreateEntity()
    {
        // Arrange
        var dataService = GetService();
        var runningJob = DataServiceTestHelper.CreateEntity();

        // Act
        await dataService
            .Create<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                Map(runningJob), CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
    }

    [Fact]
    public async Task Should_CreateEntityAndCallBeforeSave()
    {
        // Arrange
        var runningJob = DataServiceTestHelper.CreateEntity();
        var runningJobDto = Map(runningJob);

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<EventBridgeRunningJob>(runningJobDto)).Returns(runningJob);

        var dataService = GetService(mapper: mapper.Object);
        var beforeSave = new Mock<Action<EventBridgeRunningJob, IDbContext>>();

        // Act
        await dataService.Create(runningJobDto, beforeSave.Object, CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
        beforeSave.Verify(del => del(runningJob, ctx), Times.Once);
    }

    [Fact]
    public async Task Should_UpdateEntity()
    {
        // Arrange
        var runningJob = DataServiceTestHelper.CreateEntity();

        var dataService = await GetService(new[] { runningJob });

        runningJob.JobId = $"{randomizer.String(5, 10, 'a', 'z')}-job";
        runningJob.RuleId = $"{randomizer.String(5, 10, 'a', 'z')}-rule";

        // Act
        await dataService
            .Update<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                Map(runningJob), CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
    }

    [Fact]
    public async Task Should_UpdateEntityAndCallBeforeSave()
    {
        // Arrange
        var runningJob = DataServiceTestHelper.CreateEntity();
        var runningJobDto = Map(runningJob);

        var mapper = new Mock<IMapper>();
        mapper.Setup(m => m.Map<EventBridgeRunningJob>(runningJobDto)).Returns(runningJob);

        var dataService = await GetService(new[] { runningJob }, mapper: mapper.Object);

        runningJob.JobId = $"{randomizer.String(5, 10, 'a', 'z')}-job";
        runningJob.RuleId = $"{randomizer.String(5, 10, 'a', 'z')}-rule";

        var beforeSave = new Mock<Action<EventBridgeRunningJob, IDbContext>>();

        // Act
        await dataService.Update(runningJobDto, beforeSave.Object, CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
        beforeSave.Verify(del => del(runningJob, ctx), Times.Once);
    }

    [Fact]
    public async Task Should_GetEntityByKey()
    {
        // Arrange
        var runningJob = DataServiceTestHelper.CreateEntity();
        var dataService = await GetService(new[] { runningJob });

        // Act
        var runningJobDto = await dataService
            .Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                runningJob.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(runningJobDto);
        Assert.Equal(runningJob, Map(runningJobDto), this);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Should_GetEntityByKeyAndQueryHandler(bool findJob)
    {
        var entityQuery = new Mock<IEntityQuery<EventBridgeRunningJob>>();
        return Should_GetEntityByKeyAndEtc(
            findJob,
            jobId =>
            {
                entityQuery
                    .Setup(q => q.GetEntityQuery(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
                    .Returns((IQueryable<EventBridgeRunningJob> q) => q.Where(j => j.JobId == jobId))
                    .Verifiable();
                return entityQuery;
            },
            (dataService, id) =>
                dataService
                    .Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                        id, entityQuery.Object, CancellationToken.None));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Should_GetEntityByKeyAndSetQuery(bool findJob)
    {
        var setQuery = new Mock<Func<IQueryable<EventBridgeRunningJob>, IQueryable<EventBridgeRunningJob>>>();
        return Should_GetEntityByKeyAndEtc(
            findJob,
            jobId =>
            {
                setQuery
                    .Setup(sq => sq(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
                    .Returns((IQueryable<EventBridgeRunningJob> q) => q.Where(j => j.JobId == jobId))
                    .Verifiable();
                return setQuery;
            },
            (dataService, id) =>
                dataService
                    .Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                        id, setQuery.Object, CancellationToken.None));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_GetEntityBySetQuery(bool findJob)
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities();
        var dataService = await GetService(runningJobs);

        var runningJob = runningJobs[randomizer.Number(0, 9)];
        var (jobId, ruleId) = findJob
            ? (runningJob.JobId, runningJob.RuleId)
            : ("NO-JOB-ID", "NO-RULE-ID");

        var setQuery = new Mock<Func<IQueryable<EventBridgeRunningJob>, IQueryable<EventBridgeRunningJob>>>();
        setQuery
            .Setup(sq => sq(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
            .Returns(
                (IQueryable<EventBridgeRunningJob> q) =>
                    q.Where(j => j.JobId == jobId && j.RuleId == ruleId))
            .Verifiable();

        // Act
        var runningJobDto = await dataService
            .Get<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                setQuery.Object, CancellationToken.None);

        // Assert
        if (findJob)
        {
            Assert.NotNull(runningJobDto);
            Assert.Equal(runningJob, Map(runningJobDto), this);
        }
        else
        {
            Assert.Null(runningJobDto);
        }

        setQuery.Verify();
    }

    [Fact]
    public async Task Should_GetAll()
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities();
        var dataService = await GetService(runningJobs);

        // Act
        var runningJobDtos = await dataService
            .GetAll<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(CancellationToken.None);

        // Assert
        Assert.Equal(10, runningJobDtos.Count());
        AssertSequences(runningJobs, runningJobDtos.Select(Map));
    }

    [Fact]
    public Task Should_GetAllWithFilter()
        => Should_GetAllWithEtc(
            dataService => dataService
                .GetAll<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                    new Filter
                    {
                        Filters = new List<FilterInfoBase>
                        {
                                new StringFilter
                                {
                                    MatchMode = StringFilterMatchMode.StartsWith,
                                    PropertyName = nameof(EventBridgeRunningJob.JobId),
                                    Value = JOBID_PREFIX,
                                },
                        },
                    },
                    CancellationToken.None));

    [Fact]
    public async Task Should_GetAllWithSetQuery()
    {
        var setQuery = new Mock<Func<IQueryable<EventBridgeRunningJob>, IQueryable<EventBridgeRunningJob>>>();
        setQuery
            .Setup(sq => sq(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
            .Returns((IQueryable<EventBridgeRunningJob> q) => q.Where(j => j.JobId.StartsWith(JOBID_PREFIX)))
            .Verifiable();

        await Should_GetAllWithEtc(
            dataService => dataService
                .GetAll<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                    setQuery.Object, CancellationToken.None));

        setQuery.Verify();
    }

    [Fact]
    public Task Should_DeleteByKey()
        => Should_DeleteByKeyAndEtc(
            (dataService, id) =>
                dataService.Delete<EventBridgeRunningJob>(id, CancellationToken.None));

    [Fact]
    public async Task Should_DeleteByKeyAndCallBeforeSave()
    {
        var beforeSave = new Mock<Action<EventBridgeRunningJob, IDbContext>>();
        beforeSave
            .Setup(del => del(It.IsAny<EventBridgeRunningJob>(), It.IsAny<IDbContext>()))
            .Verifiable();

        await Should_DeleteByKeyAndEtc(
            (dataService, id) =>
                dataService.Delete(id, beforeSave.Object, CancellationToken.None));

        beforeSave.Verify();
    }

    [Fact]
    public async Task Should_DeleteAll()
    {
        // Arrange
        var dataService = await GetService(DataServiceTestHelper.CreateEntities());

        // Act
        await dataService.DeleteAll<EventBridgeRunningJob>(CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;
        Assert.Empty(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
    }

    [Fact]
    public async Task Should_DeleteAllWithSetQuery()
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities();
        var runningJob = runningJobs.FirstOrDefault(j => j.Id == 1);

        runningJobs
            .Where(j => j.Id != 1)
            .ToList().ForEach(j => j.JobId = JOBID_PREFIX + j.JobId);

        var setQuery = new Mock<Func<IQueryable<EventBridgeRunningJob>, IQueryable<EventBridgeRunningJob>>>();
        setQuery
            .Setup(del => del(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
            .Returns(
                (IQueryable<EventBridgeRunningJob> q) =>
                    q.Where(j => j.JobId.StartsWith(JOBID_PREFIX)))
            .Verifiable();

        var dataService = await GetService(runningJobs);

        // Act
        await dataService.DeleteAll(setQuery.Object, CancellationToken.None);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
        setQuery.Verify();
    }

    [Theory]
    [MemberData(
        nameof(DataServiceTestData.GetPageTestData_QueryHandler),
        MemberType = typeof(DataServiceTestData))]
    public async Task Should_GetPageWithQueryHandler(
        EventBridgeRunningJob[] jobs,
        QueryCommand command,
        List<EventBridgeRunningJob> expected,
        int expectedTotal,
        IEntityQuery<EventBridgeRunningJob> queryHandler)
    {
        // Arrange
        var dataService = await GetService(jobs);

        // Act
        var page = await dataService
            .GetPage<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                command, queryHandler, CancellationToken.None, false);

        // Assert
        Assert.NotNull(page);
        Assert.Equal(expectedTotal, page.Total);
        Assert.Equal(expected, page.Items.Select(Map), this);
    }

    [Theory]
    [MemberData(
        nameof(DataServiceTestData.GetPageTestData),
        MemberType = typeof(DataServiceTestData))]
    public async Task Should_GetPage(
        EventBridgeRunningJob[] jobs,
        QueryCommand command,
        List<EventBridgeRunningJob> expected,
        int expectedTotal)
    {
        // Arrange
        var dataService = await GetService(jobs);

        // Act
        var page = await dataService
            .GetPage<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(
                command, CancellationToken.None, false);

        // Assert
        Assert.NotNull(page);
        Assert.Equal(expectedTotal, page.Total);
        Assert.Equal(expected, page.Items.Select(Map), this);
    }

    private async Task<IDataService> GetService(
        EventBridgeRunningJob[] jobs,
        IDbContext context = default,
        IMapper mapper = default)
    {
        var dataService = GetService(context, mapper);

        jobs ??= new EventBridgeRunningJob[0];
        if (jobs.Length > 0)
        {
            await dataService.Context.Set<EventBridgeRunningJob>().AddRangeAsync(jobs);
            await dataService.Context.SaveChangesAsync();
        }

        return dataService;
    }

    private IDataService GetService(IDbContext context = default, IMapper mapper = default)
        => new DataService(context ?? SutDataHelper.CreateEmptyContext(), mapper ?? Mapper);

    private AwsEventBridgeRunningJobDTO Map(EventBridgeRunningJob j)
        => Mapper.Map<AwsEventBridgeRunningJobDTO>(j);

    private EventBridgeRunningJob Map(AwsEventBridgeRunningJobDTO j)
        => Mapper.Map<EventBridgeRunningJob>(j);

    private async Task Should_GetEntityByKeyAndEtc<TMock>(
        bool findJob,
        Func<string, Mock<TMock>> getMock,
        Func<IDataService, int, Task<AwsEventBridgeRunningJobDTO>> getRunningJob)
        where TMock : class
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities();
        var dataService = await GetService(runningJobs);

        var runningJob = runningJobs[randomizer.Number(0, 9)];
        var jobId = findJob ? runningJob.JobId : "NO-JOB-ID";
        var etcMock = getMock(jobId);

        // Act
        var runningJobDto = await getRunningJob(dataService, runningJob.Id);

        // Assert
        if (findJob)
        {
            Assert.NotNull(runningJobDto);
            Assert.Equal(runningJob, Map(runningJobDto), this);
        }
        else
        {
            Assert.Null(runningJobDto);
        }

        etcMock.Verify();
    }

    private async Task Should_GetAllWithEtc(
        Func<IDataService, Task<IEnumerable<AwsEventBridgeRunningJobDTO>>> getRunningJobs)
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities();
        var filteredRunningJobs =
            new[]
            {
                    runningJobs[randomizer.Number(0, 3)],
                    runningJobs[randomizer.Number(4, 7)],
                    runningJobs[randomizer.Number(8, 9)],
            }
            .ToList();
        filteredRunningJobs.ForEach(j => j.JobId = JOBID_PREFIX + j.JobId);

        var dataService = await GetService(runningJobs);

        // Act
        var runningJobDtos = await getRunningJobs(dataService);

        // Assert
        Assert.Equal(3, runningJobDtos.Count());
        AssertSequences(filteredRunningJobs, runningJobDtos.Select(Map));
    }

    private async Task Should_DeleteByKeyAndEtc(Func<IDataService, int, Task> deleteJob)
    {
        // Arrange
        var runningJobs = DataServiceTestHelper.CreateEntities(2);
        var dataService = await GetService(runningJobs);

        var runningJob = runningJobs.FirstOrDefault(j => j.Id == 2);

        // Act
        await deleteJob(dataService, 1);

        // Assert
        using var ctx = dataService.Context;

        var runningJob2 = Assert.Single(await ctx.Set<EventBridgeRunningJob>().ToListAsync());
        Assert.Equal(runningJob, runningJob2, this);
    }

    private void AssertSequences(
        IEnumerable<EventBridgeRunningJob> s1,
        IEnumerable<EventBridgeRunningJob> s2)
    => Assert.Equal(s1.OrderBy(j => j.Id), s2.OrderBy(j => j.Id), this);
}
