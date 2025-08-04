using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Scheduler.Model;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BBWM.Scheduler.Jobs;

public class JobHelper
{
    private readonly IDbContext _dbContext;

    public JobHelper(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public static async Task<bool> IsRecurringJob(IScheduler scheduler, JobKey jobKey)
    {
        var triggers = await scheduler.GetTriggersOfJob(jobKey);

        foreach (var trigger in triggers)
        {
            if (trigger is ICronTrigger || trigger is ISimpleTrigger simpleTrigger && simpleTrigger.RepeatCount != 0)
            {
                return true;
            }
        }

        return false;
    }

    public async Task UpdateJobExecutionDetailsAsync(string jobName, string status, bool success, string message, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.Set<JobRunDetails>()
                                  .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (job != null)
        {
            job.Status = status;
            job.Success = success;
            job.Message = message;
            job.LastModified = DateTime.Now;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new BusinessException($"Job: {jobName} not found.");
        }
    }
}
