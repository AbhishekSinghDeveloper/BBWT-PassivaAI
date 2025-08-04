using BBWM.Core.Filters;
using BBWM.Scheduler.DTO;

namespace BBWM.Scheduler.Service;

public interface ISchedulerJobService
{
    Task<IEnumerable<object>> GetOverviewAsync(string? view, CancellationToken ct);
    Task<PageResultDTO> GetJobsByStatusAsync(string status, QueryCommand command, CancellationToken ct);
    Task<JobRunDetailsDTO> GetJobDetailsAsync(int jobId, CancellationToken ct);
    Task<PageResultDTO> GetRecurringJobsAsync(QueryCommand command, CancellationToken ct);
    Task<PageResultDTO> GetRetriedJobsAsync(QueryCommand command, CancellationToken ct);
    Task<PageServerResultDTO> GetServersAsync(QueryCommand command, CancellationToken ct);
    Task<bool> PauseJobAsync(int jobId, CancellationToken ct);
    Task<bool> ResumeJobAsync(int jobId, CancellationToken ct);
    Task<bool> RetryJobAsync(int jobId, CancellationToken ct);
    Task<bool> DeleteJobAsync(int jobId, CancellationToken ct);
    Task<bool> TriggerJobAsync(int jobId, string message, CancellationToken ct);
    Task<bool> RuleExistsAsync(string name, CancellationToken ct);
    Task<bool> SaveJobAsync(string jobName, string cronExpression, CancellationToken ct = default);
}

