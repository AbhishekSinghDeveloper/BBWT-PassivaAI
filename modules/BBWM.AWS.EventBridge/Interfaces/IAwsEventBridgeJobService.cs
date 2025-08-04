using BBWM.AWS.EventBridge.DTO;

using System.Linq.Expressions;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IAwsEventBridgeJobService
{
    Task<AwsEventBridgeJobDTO> FindByRuleAsync(string ruleId, CancellationToken ct = default);

    Task StartJobAsync(AwsEventBridgeStartJobDTO startInfo, CancellationToken ct = default);

    Task RestartJobAsync(int historyId, CancellationToken ct = default);

    void RegisterJob<TJob>() where TJob : IEventBridgeJob;

    Task<List<AwsEventBridgeJobInfoDTO>> GetJobsListAsync(CancellationToken ct = default);

    Task<List<AwsEventBridgeJobDTO>> GetAllAsync(
        Expression<Func<AwsEventBridgeJobDTO, bool>> filter, CancellationToken ct = default);

    bool IsJobRegistered(string jobId);

    AwsEventBridgeJobInfoDTO GetJobInfo(string jobId);
}
