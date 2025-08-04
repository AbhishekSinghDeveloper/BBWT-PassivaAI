using BBWM.Core.Data;
using BBWM.Scheduler.Jobs;
using BBWM.Scheduler.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;

namespace BBWM.Scheduler;

public class JobCompletionListener : IJobListener
{
    private readonly IDbContext _dbContext;
    private readonly ILogger<TestReportingJob> _logger;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public JobCompletionListener(IDbContext dbContext, ILogger<TestReportingJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public string Name => "JobCompletionListener";

    // Called before the job is executed
    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync();

        try
        {
            var jobKey = context.JobDetail.Key;
            var jobName = jobKey.Name;

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.JobName == jobName);

            if (job == null)
            {
                _logger.LogWarning($"JobToBeExecuted(): Job with name '{jobName}' was not found.");
                return;
            }

            job.Status = "Processing";
            job.LastModified = DateTime.Now;
            job.Cron = GetCronExpression(context);
            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Called if the job execution was vetoed
    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync();
        try
        {
            var jobKey = context.JobDetail.Key;
            var jobName = jobKey.Name;

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.JobName == jobName);

            if (job == null)
            {
                _logger.LogWarning($"JobExecutionVetoed(): Job with name '{jobName}' was not found.");
                return;
            }

            job.Status = "Vetoed";
            job.Success = false;
            job.LastModified = DateTime.Now;
            job.Message = "Job execution was vetoed.";
            job.Cron = GetCronExpression(context);

            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Called after the job has been executed
    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync();
        try
        {
            var jobKey = context.JobDetail.Key;
            var jobName = jobKey.Name;

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.JobName == jobName);

            if (job == null)
            {
                _logger.LogWarning($"JobWasExecuted(): Job with name '{jobName}' was not found.");
                return;
            }

            if (jobException == null)
            {
                job.Status = "Succeeded";
                job.Success = true;
                job.Message = "Job completed successfully.";
            }
            else
            {
                job.Status = "Failed";
                job.Success = false;
                job.Message = $"Job failed: {jobException.Message}";
            }

            job.LastModified = DateTime.Now;
            job.Cron = GetCronExpression(context);

            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public string GetCronExpression(IJobExecutionContext context)
    {
        var trigger = context.Trigger;

        string cronExpression = null;
        if (trigger is CronTriggerImpl cronTrigger)
        {
            cronExpression = cronTrigger.CronExpressionString;
        }

        return cronExpression;
    }
}
