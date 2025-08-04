using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Services;

using Moq;

namespace BBWM.Core.Test.Services;

public static class DataServiceTestData
{
    private const string TARGET1 = "_MyDummyJob_1";
    private const string TARGET2 = "_MyDummyJob_2";
    private const string TARGET3 = "_MyDummyJob_3";

    private const int JOB1_ID = 17;
    private const int JOB2_ID = 35;
    private const int JOB3_ID = 48;
    private const int JOB4_ID = 60;
    private const int JOB5_ID = 89;

    private const string JOB1_TARGET = TARGET1;
    private const string JOB2_TARGET = TARGET2;
    private const string JOB3_TARGET = TARGET3;
    private const string JOB4_TARGET = TARGET1;
    private const string JOB5_TARGET = TARGET2;

    private const string JOB1_RULEID = "_MyDummyRule_CONTAINSPATTERNSEARCH_1";
    private const string JOB2_RULEID = "_MyDummyRule_DEF_2";
    private const string JOB3_RULEID = "_MyDummyRule_GHI_3";
    private const string JOB4_RULEID = "_MyDummyRule_CONTAINSPATTERNSEARCH_4";
    private const string JOB5_RULEID = "_MyDummyRule_GHI_5";

    public static List<object[]> GetPageTestData
    {
        get
        {
            var noFilterJobs = CreateJobsList();
            var cmdNullJobs = CreateJobsList();

            return new List<object[]>
                {
                    new object[]
                    {
                        CreateJobsList(),
                        new QueryCommand
                        {
                            Filters = new List<FilterInfoBase>
                            {
                                new StringFilter
                                {
                                    MatchMode = StringFilterMatchMode.StartsWith,
                                    PropertyName = nameof(EventBridgeRunningJob.RuleId),
                                    Value = "_MyDummyRule_",
                                },
                            },
                            SortingField = nameof(EventBridgeRunningJob.Id),
                            SortingDirection = OrderDirection.Desc,
                        },
                        DefaultJobs.Values.OrderByDescending(j => j.Id).ToList(),
                        DefaultJobs.Count,
                    },
                    new object[]
                    {
                        noFilterJobs,
                        new QueryCommand
                        {
                            SortingField = nameof(EventBridgeRunningJob.Id),
                            SortingDirection = OrderDirection.Desc,
                        },
                        noFilterJobs.OrderByDescending(j => j.Id).ToList(),
                        noFilterJobs.Length,
                    },
                    new object[]
                    {
                        noFilterJobs,
                        null,
                        noFilterJobs.ToList(),
                        noFilterJobs.Length,
                    },
                    new object[]
                    {
                        CreateJobsList(),
                        new QueryCommand
                        {
                            Filters = new List<FilterInfoBase>
                            {
                                new StringFilter
                                {
                                    MatchMode = StringFilterMatchMode.StartsWith,
                                    PropertyName = nameof(EventBridgeRunningJob.RuleId),
                                    Value = "_MyDummyRule_",
                                },
                                new StringFilter
                                {
                                    MatchMode = StringFilterMatchMode.Contains,
                                    PropertyName = nameof(EventBridgeRunningJob.RuleId),
                                    Value = "CONTAINSPATTERNSEARCH",
                                },
                            },
                            SortingField = nameof(EventBridgeRunningJob.Id),
                            SortingDirection = OrderDirection.Desc,
                            Take = 1,
                        },
                        DefaultJobs.Values
                            .Where(
                                j => j.RuleId.StartsWith("_MyDummyRule_") || j.RuleId.Contains("CONTAINSPATTERNSEARCH"))
                            .OrderByDescending(j => j.Id)
                            .Take(1)
                            .ToList(),
                        DefaultJobs.Values
                            .Where(
                                j => j.RuleId.StartsWith("_MyDummyRule_") || j.RuleId.Contains("CONTAINSPATTERNSEARCH"))
                            .Count(),
                    },
                    new object[]
                    {
                        CreateJobsList(),
                        new QueryCommand
                        {
                            Filters = new List<FilterInfoBase>
                            {
                                new StringFilter
                                {
                                    MatchMode = StringFilterMatchMode.StartsWith,
                                    PropertyName = nameof(EventBridgeRunningJob.RuleId),
                                    Value = "_MyDummyRule_",
                                },
                            },
                            Skip = 2,
                            Take = 2,
                            SortingField = nameof(EventBridgeRunningJob.Id),
                            SortingDirection = OrderDirection.Asc,
                        },
                        DefaultJobs.Values.OrderBy(j => j.Id).Skip(2).Take(2).ToList(),
                        DefaultJobs.Count,
                    },
                    new object[]
                {
                    CreateJobsList(),
                    new QueryCommand
                    {
                        Filters = new List<FilterInfoBase>
                        {
                            new StringFilter
                            {
                                MatchMode = StringFilterMatchMode.Equals,
                                PropertyName = nameof(EventBridgeRunningJob.JobId),
                                Value = TARGET1,
                            },
                        },
                        SortingField = nameof(EventBridgeRunningJob.Id),
                        SortingDirection = OrderDirection.Asc,
                    },
                    DefaultJobs.Values.Where(j => j.JobId == TARGET1).OrderBy(j => j.Id).ToList(),
                    DefaultJobs.Values.Where(j => j.JobId == TARGET1).Count(),
                },
                };
        }
    }

    public static List<object[]> GetPageTestData_QueryHandler
    {
        get
        {
            var queryHandler = new Mock<IEntityQuery<EventBridgeRunningJob>>();
            queryHandler
                .Setup(qh => qh.GetEntityQuery(It.IsAny<IQueryable<EventBridgeRunningJob>>()))
                .Returns((IQueryable<EventBridgeRunningJob> q) => q);

            return GetPageTestData.Select(arr => arr.Append(queryHandler.Object).ToArray()).ToList();
        }
    }

    private static Dictionary<int, EventBridgeRunningJob> _defaultJobs = null;

    private static readonly Dictionary<int, EventBridgeRunningJob> DefaultJobs =
        _defaultJobs ??=
            new Dictionary<int, EventBridgeRunningJob>
            {
                [JOB1_ID] =
                    new EventBridgeRunningJob
                    {
                        Id = JOB1_ID,
                        RuleId = JOB1_RULEID,
                        JobId = JOB1_TARGET,
                    },
                [JOB2_ID] =
                    new EventBridgeRunningJob
                    {
                        Id = JOB2_ID,
                        RuleId = JOB2_RULEID,
                        JobId = JOB2_TARGET,
                    },
                [JOB3_ID] =
                    new EventBridgeRunningJob
                    {
                        Id = JOB3_ID,
                        RuleId = JOB3_RULEID,
                        JobId = JOB3_TARGET,
                    },
                [JOB4_ID] =
                    new EventBridgeRunningJob
                    {
                        Id = JOB4_ID,
                        RuleId = JOB4_RULEID,
                        JobId = JOB4_TARGET,
                    },
                [JOB5_ID] =
                    new EventBridgeRunningJob
                    {
                        Id = JOB5_ID,
                        RuleId = JOB5_RULEID,
                        JobId = JOB5_TARGET,
                    },
            };

    private static EventBridgeRunningJob[] CreateJobsList()
        => Enumerable
            .Range(1, 100)
            .Select(
                id => DefaultJobs.TryGetValue(id, out var runningJob)
                    ? runningJob
                    : DataServiceTestHelper.CreateEntity(id))
            .ToArray();
}
