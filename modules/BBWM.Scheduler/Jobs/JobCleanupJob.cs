using Quartz;
using Microsoft.Extensions.Logging;
using BBWM.Core.Data;
using Microsoft.EntityFrameworkCore;
using BBWM.Scheduler.Model;

namespace BBWM.Scheduler.Jobs;

public class JobCleanupJob : IJob
{
    private readonly IDbContext _context;
    private readonly ILogger<JobCleanupJob> _logger;

    public JobCleanupJob(IDbContext context, ILogger<JobCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting job cleanup...");

        var cutoffDate = SchedulerSettings.JobCleanupCutoffDate;

        try
        {
            var oldJobs = await _context.Set<JobRunDetails>()
                .Where(j => j.LastModified < cutoffDate)
                .ToListAsync();

            if (oldJobs.Any())
            {
                _context.Set<JobRunDetails>().RemoveRange(oldJobs);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"{oldJobs.Count} jobs older than {cutoffDate} have been deleted.");
            }
            else
            {
                _logger.LogInformation("No jobs found for cleanup.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during job cleanup.");
        }
    }
}
