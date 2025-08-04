using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;

namespace BBWM.AWS.EventBridge.Test.Service;

public static class AwsEventBridgeJobHistoryServiceTestData
{
    private static readonly Random dateRandom = new Random();
    private static readonly DateTime BaseDate = DateTime.UtcNow.AddYears(-1);

    private static List<EventBridgeJobHistory> History => new List<EventBridgeJobHistory>
        {
            new EventBridgeJobHistory
            {
                JobId = "SuccessJob",
                RuleId = "MyRule1",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.Succeed,
            },
            new EventBridgeJobHistory
            {
                JobId = "CanceledJob1",
                RuleId = "MyRule2",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.CanceledByUser,
            },
            new EventBridgeJobHistory
            {
                JobId = "CanceledJob2",
                RuleId = "MyRule3",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.CanceledByUser,
            },
            new EventBridgeJobHistory
            {
                JobId = "CanceledJob1",
                RuleId = "MyRule2",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.CanceledByShutdown,
            },
            new EventBridgeJobHistory
            {
                JobId = "CanceledJob2",
                RuleId = "MyRule3",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.CanceledByUser,
            },
            new EventBridgeJobHistory
            {
                JobId = "FailedJob",
                RuleId = "MyRule4",
                StartTime = RandomStartTime,
                CompletionStatus = JobCompletionStatus.Failed,
            },
        };

    private static DateTime RandomStartTime
        => BaseDate
            .AddDays(dateRandom.Next(365))
            .AddMinutes(dateRandom.Next(60))
            .AddSeconds(dateRandom.Next(60))
            .AddMilliseconds(dateRandom.Next(1000));

    public static List<EventBridgeJobHistory> CreateHistoryData()
    {
        var history = History;

        history.ForEach(h =>
        {
            var v = dateRandom.Next(3);
            h.FinishTime = v == 0
            ? h.StartTime.AddMilliseconds(dateRandom.Next(1000))
            : v == 1
                ? h.StartTime.AddSeconds(dateRandom.Next(60))
                : h.StartTime.AddMinutes(dateRandom.Next(10));
        });

        return history;
    }

    public static List<object[]> CanceledJobsPageExpectedResults => new List<object[]>
        {
            new object[] { 4, null }, // All canceled jobs
            new object[] // Canceled job filtered by name
            {
                2,
                new QueryCommand
                {
                    Skip = 0,
                    Take = 2,
                    SortingDirection = OrderDirection.Asc,
                    SortingField = nameof(AwsEventBridgeJobHistoryDTO.RuleId),
                    Filters = new List<FilterInfoBase>
                    {
                        new StringFilter
                        {
                            MatchMode = StringFilterMatchMode.StartsWith,
                            PropertyName = nameof(AwsEventBridgeJobHistoryDTO.RuleId),
                            Value = "MyRule2",
                        },
                    },
                },
            },
            new object[] // Zero canceled job
            {
                0,
                new QueryCommand
                {
                    Skip = 0,
                    Take = 2,
                    SortingDirection = OrderDirection.Asc,
                    SortingField = nameof(AwsEventBridgeJobHistoryDTO.RuleId),
                    Filters = new List<FilterInfoBase>
                    {
                        new StringFilter
                        {
                            MatchMode = StringFilterMatchMode.StartsWith,
                            PropertyName = nameof(AwsEventBridgeJobHistoryDTO.RuleId),
                            Value = "MyRule1",
                        },
                    },
                },
            },
        };
}
