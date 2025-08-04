using Amazon.EventBridge.Model;

using Autofac.Features.OwnedInstances;

using BBWM.AWS.EventBridge.AwsCron;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace BBWM.AWS.EventBridge;

public static class JobExecutionWrapper
{

    private static readonly ConcurrentDictionary<Guid, RuntimeJobRunningInfo> runtimeInfo =
        new ConcurrentDictionary<Guid, RuntimeJobRunningInfo>();
    private static readonly object updateInfoLock = new object();
    private static readonly object shuttingDownJobLock = new object();
    private static readonly object shutdownCompleteLock = new object();
    private static bool shuttingDown = false;
    private static bool shutdownComplete = false;

    private static bool ShuttingDown
    {
        get
        {
            lock (shuttingDownJobLock)
            { return shuttingDown; }
        }
        set
        {
            lock (shuttingDownJobLock)
            { shuttingDown = value; }
        }
    }

    private static bool ShutdownComplete
    {
        get
        {
            lock (shutdownCompleteLock)
            { return shutdownComplete; }
        }
        set
        {
            lock (shutdownCompleteLock)
            { shutdownComplete = value; }
        }
    }

    public static void TrackJobExecution(
        IServiceScope serviceScope, string ruleId, Type jobType, List<AwsEventBridgeJobParameterDTO> @params)
    {
        if (!typeof(IEventBridgeJob).IsAssignableFrom(jobType) || ShuttingDown || ShutdownComplete)
        { return; }

        var factory = serviceScope.ServiceProvider.GetRequiredService<TrackingJobContext.Factory>();
        var ownedContext = factory(jobType, @params);

        Task.Run(() => TrackJobAsync(ruleId, ownedContext));
    }

    public static void CancelByShutdown()
    {
        ShuttingDown = true;

        if (runtimeInfo.Count == 0)
        { return; }

        foreach (var (id, info) in runtimeInfo)
        {
            UpdateRuntimeInfo(() => info.CompletionStatus = JobCompletionStatus.CanceledByShutdown);
            info.TokenSource.Cancel();
        }

        while (!ShutdownComplete)
        { Thread.Sleep(100); }
    }

    public static void CancelByUser(Guid cancelationId)
    {
        if (runtimeInfo.TryGetValue(cancelationId, out var info))
        {
            UpdateRuntimeInfo(() => info.CompletionStatus = JobCompletionStatus.CanceledByUser);
            info.TokenSource.Cancel();
        }
    }

    private static void UpdateRuntimeInfo(Action update)
    {
        lock (updateInfoLock)
        { update(); }
    }

    private static async Task TrackJobAsync(string ruleId, Owned<TrackingJobContext> ownedContext)
    {
        using (ownedContext)
        {
            var ctx = ownedContext.Value;
            var job = ctx.Job;
            var @params = ctx.Parameters;

            var start = DateTime.UtcNow;
            var history = new AwsEventBridgeJobHistoryDTO
            {
                StartTime = start,
                JobId = ctx.JobMetadata.JobId,
                CompletionStatus = JobCompletionStatus.Succeed,
                RuleId = ruleId,
                Parameters = @params
            };
            AwsEventBridgeRunningJobDTO runningJob = null;
            AwsEventBridgeJobDTO ourJob = default;
            string jobCron = default;

            try
            {
                ourJob = await ctx.JobService.FindByRuleAsync(ruleId, CancellationToken.None);
                jobCron = await GetJobCronAsync(ctx.ClientFactory, ruleId, CancellationToken.None);
                if (ourJob is null || string.IsNullOrEmpty(jobCron))
                { return; } // This shouldn't happen

                runningJob = await CreateRunningJobAsync(
                    ctx.DataService, ctx.JobMetadata.JobId, ruleId, start, CancellationToken.None);

                var runtimeJobInfo = new RuntimeJobRunningInfo
                { TokenSource = new CancellationTokenSource() };
                runtimeInfo[runningJob.CancelationId] = runtimeJobInfo;

                await RunJobAsync(
                    job, ruleId, runningJob.CancelationId, ctx, history, @params, runtimeJobInfo.TokenSource.Token);

                ourJob.LastExecutionTime = DateTime.UtcNow;
            }
            catch (Exception e)
            { ctx.Logger?.LogError(e, "Error while tracking \"{RuleId} ({ParamsFormat})\"", ruleId, @params.Format()); }
            finally
            { await EndTrackingAsync(ctx, history, runningJob, ourJob, jobCron, CancellationToken.None); }
        }
    }

    private static async Task RunJobAsync(
        IEventBridgeJob job,
        string ruleId,
        Guid cancelationId,
        TrackingJobContext ctx,
        AwsEventBridgeJobHistoryDTO history,
        List<AwsEventBridgeJobParameterDTO> @params,
        CancellationToken cancellationToken)
    {
        try
        {
            await job.RunAsync(@params, cancellationToken);
        }
        catch (OperationCanceledException oce)
        when (oce.CancellationToken == cancellationToken && cancellationToken.IsCancellationRequested)
        {
            var status = JobCompletionStatus.Unknown;
            if (runtimeInfo.TryGetValue(cancelationId, out var info) &&
                Enum.IsDefined(typeof(JobCompletionStatus), info.CompletionStatus))
            { status = info.CompletionStatus; }

            ctx.ErrorHandler.HandleJobCancelationSafely(
                ruleId, job, @params, status == JobCompletionStatus.CanceledByUser, ctx.Logger);

            history.CompletionStatus = status;
        }
        catch (Exception error)
        {
            ctx.ErrorHandler.HandleJobFailureSafely(ruleId, job, @params, error, ctx.Logger);

            history.CompletionStatus = JobCompletionStatus.Failed;
            history.ErrorMessage = error.Message;
            history.StackTrace = error.StackTrace;
            throw;
        }
    }

    private static async Task EndTrackingAsync(
        TrackingJobContext ctx,
        AwsEventBridgeJobHistoryDTO history,
        AwsEventBridgeRunningJobDTO runningJob,
        AwsEventBridgeJobDTO ourJob,
        string jobCron,
        CancellationToken ct)
    {
        if (ourJob is null || string.IsNullOrEmpty(jobCron))
        { return; }

        ourJob.NextExecutionTime = AwsCronExpression.Parse(jobCron).GetNextOccurrence(DateTime.UtcNow);
        await ctx.DataService.Update<EventBridgeJob, AwsEventBridgeJobDTO>(ourJob, ct);

        await ctx.DataService.Delete<EventBridgeRunningJob>(runningJob.Id, ct);

        history.FinishTime = DateTime.UtcNow;
        await ctx.DataService.Create<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(history, ct);

        if (runtimeInfo.TryRemove(runningJob.CancelationId, out var info))
        { using (info.TokenSource) { /* do nothing */ } }

        if (ShuttingDown && runtimeInfo.Count == 0)
        { ShutdownComplete = true; }
    }

    private static async Task<string> GetJobCronAsync(
        IAwsEventBridgeClientFactory clientFactory, string ruleId, CancellationToken cancellationToken)
    {
        try
        {
            var client = clientFactory.CreateClient();
            var rule = await client.DescribeRuleAsync(
                new DescribeRuleRequest
                { Name = ruleId }, cancellationToken);
            return rule.ScheduleExpression;
        }
        catch
        { return default; }
    }

    private static async Task<AwsEventBridgeRunningJobDTO> CreateRunningJobAsync(
        IDataService dataService,
        string jobId,
        string ruleId,
        DateTime start,
        CancellationToken cancellationToken)
    {
        var runningJob = new AwsEventBridgeRunningJobDTO
        {
            JobId = jobId,
            RuleId = ruleId,
            StartTime = start
        };

        runningJob = await dataService.Create<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(runningJob, cancellationToken);

        return runningJob;
    }

    private static void HandleJobFailureSafely(
        this IEventBridgeJobErrorHandler handler,
        string ruleId,
        IEventBridgeJob sourceJob,
        List<AwsEventBridgeJobParameterDTO> @params,
        Exception error,
        ILogger<IJobExecutionWrapper> logger)
    {
        @params ??= new List<AwsEventBridgeJobParameterDTO>();

        try
        {
            handler?.HandleJobFailure(ruleId, sourceJob, @params, error);
        }
        catch (Exception e)
        {
            if (logger is not null)
            {
                var errorMessage =
                    $"Job error handler failed processing failure for \"{ruleId} ({@params.Format()})\"";
                logger.LogError(e, errorMessage);
            }
        }
    }

    private static void HandleJobCancelationSafely(
        this IEventBridgeJobErrorHandler handler,
        string ruleId,
        IEventBridgeJob sourceJob,
        List<AwsEventBridgeJobParameterDTO> @params,
        bool canceledByUser,
        ILogger<IJobExecutionWrapper> logger)
    {

        @params ??= new List<AwsEventBridgeJobParameterDTO>();

        try
        {
            handler?.HandleJobCancelation(ruleId, sourceJob, @params, canceledByUser);
        }
        catch (Exception e)
        {
            if (logger is not null)
            {
                var errorMessage =
                    $"Job error handler failed processing cancelation for \"{ruleId} ({@params.Format()})\"";
                logger.LogError(e, errorMessage);
            }
        }
    }

    private static string Format(this List<AwsEventBridgeJobParameterDTO> @params)
        => @params is not null
            ? string.Join(", ", @params.Where(p => p is not null).Select(p => $"{p.Name}: {p.Value}"))
            : string.Empty;
}
