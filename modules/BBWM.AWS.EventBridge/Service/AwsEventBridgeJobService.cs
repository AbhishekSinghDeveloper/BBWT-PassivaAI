using Amazon.EventBridge;
using Amazon.EventBridge.Model;

using AutoMapper;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.Core.Utils;

using Microsoft.Extensions.DependencyInjection;

using System.Data;
using System.Linq.Expressions;

namespace BBWM.AWS.EventBridge.Service;

public class AwsEventBridgeJobService : IAwsEventBridgeJobService
{
    private class JobInfo
    {
        public string Description { get; set; }
        public Type JobType { get; set; }
        public List<JobParameterInfo> Parameters { get; set; }
        public Action<IServiceScope, string, Type, List<AwsEventBridgeJobParameterDTO>> Action { get; set; }
    }

    private static readonly Dictionary<string, JobInfo> JobsMap = new();

    private readonly IMapper mapper;
    private readonly IAwsEventBridgeClientFactory clientFactory;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IDataService dataService;

    public AwsEventBridgeJobService(
        IMapper mapper,
        IDataService dataService,
        IAwsEventBridgeClientFactory clientFactory,
        IServiceScopeFactory scopeFactory)
    {
        this.mapper = mapper;
        this.dataService = dataService;
        this.clientFactory = clientFactory;
        this.scopeFactory = scopeFactory;
    }

    public void RegisterJob<TJob>() where TJob : IEventBridgeJob
    {
        using var scope = scopeFactory.CreateScope();
        var metadata = scope.ServiceProvider.GetRequiredService<IEventBridgeJobMetadata<TJob>>();

        var jobId = metadata.JobId;
        var description = metadata.JobDescription;
        var @params = metadata.Parameters ?? new List<JobParameterInfo>();

        if (JobsMap.ContainsKey(jobId))
        { throw new AwsEventBridgeException($"Job \"{jobId}\" has been registered already."); }

        if (@params.GroupBy(p => p.Name).Count() != @params.Count)
        { throw new AwsEventBridgeException($"Job \"{jobId}\" cannot repeat parameters."); }

        JobsMap.Add(
            jobId, new JobInfo
            {
                Description = description,
                JobType = typeof(TJob),
                Parameters = @params,
                Action = (scope, ruleId, jobType, @params) => JobExecutionWrapper.TrackJobExecution(scope, ruleId, jobType, @params)
            });
    }

    public async Task StartJobAsync(AwsEventBridgeStartJobDTO startInfo, CancellationToken ct = default)
    {
        var client = clientFactory.CreateClient();

        try
        {
            var ruleId = startInfo.RuleId;

            var rule = await client.DescribeRuleAsync(
                new DescribeRuleRequest { Name = ruleId }, ct);
            if (rule.State == RuleState.DISABLED)
            { throw new ApiException($"Rule \"{ruleId}\" is disabled."); }

            var ourJob = await FindByRuleAsync(ruleId, ct);
            CreateContextAndLaunch(ruleId, ourJob.JobId, ourJob.Parameters);
        }
        catch (ResourceNotFoundException)
        { throw new ApiException($"Rule \"{startInfo.RuleId}\" doesn't exist."); }
        catch (Exception e) when (e is not ApiException)
        { throw new ApiException($"An error occurred when starting job for rule: \"{startInfo.RuleId}\"."); }
    }

    public async Task<List<AwsEventBridgeJobInfoDTO>> GetJobsListAsync(CancellationToken ct = default)
    {
        var storedJobs = (await dataService.GetAll<EventBridgeJob, AwsEventBridgeJobDTO>(ct))
            .Select(j => j.JobId)
            .ToHashSet();

        return JobsMap
            .Select(kv => (kv.Key, kv.Value.Description, kv.Value.Parameters))
            .Select(j => new AwsEventBridgeJobInfoDTO
            {
                JobId = j.Key,
                JobDescription = j.Description,
                Available = (j.Parameters is not null && j.Parameters.Count > 0) || !storedJobs.Contains(j.Key),
                Parameters = j.Parameters
            })
            .ToList();
    }

    public async Task<List<AwsEventBridgeJobDTO>> GetAllAsync(
        Expression<Func<AwsEventBridgeJobDTO, bool>> filter, CancellationToken ct = default)
    {
        Expression<Func<EventBridgeJob, bool>> predicate = null;
        if (filter is not null)
        { predicate = ExpressionTransformer.Tranform<AwsEventBridgeJobDTO, EventBridgeJob>(filter, mapper); }

        return (await dataService
            .GetAll<EventBridgeJob, AwsEventBridgeJobDTO>(q => predicate is not null ? q.Where(predicate) : q, ct))
            .ToList();
    }

    public bool IsJobRegistered(string jobId)
        => JobsMap.ContainsKey(jobId ?? "");

    public Task<AwsEventBridgeJobDTO> FindByRuleAsync(string ruleId, CancellationToken ct = default)
        => dataService.Get<EventBridgeJob, AwsEventBridgeJobDTO>(q => q.Where(j => j.RuleId == ruleId), ct);

    public AwsEventBridgeJobInfoDTO GetJobInfo(string jobId)
    {
        if (!IsJobRegistered(jobId))
        { return default; }

        var info = JobsMap[jobId];

        return new AwsEventBridgeJobInfoDTO
        {
            JobId = jobId,
            JobDescription = info.Description,
            Available = true,
            Parameters = info.Parameters
        };
    }

    public async Task RestartJobAsync(int historyId, CancellationToken ct = default)
    {
        var history = await dataService.Get<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(historyId, ct);
        var notFound = new EntityNotFoundException("The information does not exist.");

        try
        {
            if (history is not null)
            { CreateContextAndLaunch(history.RuleId, history.JobId, history.Parameters); }
            else
            { throw notFound; }
        }
        catch (ResourceNotFoundException)
        { throw notFound; }
    }

    private void CreateContextAndLaunch(
        string ruleId,
        string targetJobId,
        List<AwsEventBridgeJobParameterDTO> parameters)
    {
        if (JobsMap.TryGetValue(targetJobId ?? "", out var info))
        {
            var jobAction = info.Action;
            var scope = scopeFactory.CreateScope();

            jobAction(scope, ruleId, info.JobType, parameters);
        }
        else
        { throw new ResourceNotFoundException(string.Empty); }
    }
}
