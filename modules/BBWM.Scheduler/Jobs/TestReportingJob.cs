using BBWM.Core.Data;
using BBWM.Scheduler.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;
using System.Diagnostics;

namespace BBWM.Scheduler.Jobs;

public class TestReportingJob : IJob
{
    private readonly ILogger<TestReportingJob> _logger;
    private readonly IDbContext _context;
    private readonly IHubContext<JobStatusHub> _hubContext;

    public TestReportingJob(IDbContext context, ILogger<TestReportingJob> logger, IHubContext<JobStatusHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        bool isRecurring = context.Trigger is ICronTrigger ||
                            (context.Trigger is ISimpleTrigger simpleTrigger && simpleTrigger.RepeatCount != 0);

        var jobType = context.JobDetail.JobType;
        var assemblyName = jobType.Assembly.GetName().Name;
        var trigger = context.Trigger;
        string cronExpression = null;

        if (trigger is CronTriggerImpl cronTrigger)
        {
            cronExpression = cronTrigger.CronExpressionString;
        }

        var jobExecutionDetails = new JobRunDetails
        {
            JobName = context.JobDetail.Key.Name,
            JobGroup = context.JobDetail.Key.Group,
            JobType = jobType.FullName,
            AssemblyName = assemblyName,
            TriggerType = context.Trigger.GetType().Name,
            TriggerGroup = context.Trigger.Key.Group,
            ExecutionTime = DateTime.Now,
            LastModified = DateTime.Now,
            Status = "Processing",
            Success = true,
            Message = "",
            ServerName = Environment.MachineName,
            IsRecurring = isRecurring,
            Cron = cronExpression
        };

        try
        {
            await Task.Delay(500);
            _logger.LogInformation($"Job executed at: {DateTime.Now}");

            jobExecutionDetails.Status = "Succeeded";
            jobExecutionDetails.Message = "Job executed successfully";
            await _hubContext.Clients.All.SendAsync("ReceiveJobStatusUpdate", "Succeeded", context.JobDetail.Key.Name, DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job failed.");

            jobExecutionDetails.Success = false;
            jobExecutionDetails.Message = GetFullExceptionMessage(ex);
            jobExecutionDetails.Status = "Failed";
            await _hubContext.Clients.All.SendAsync("ReceiveJobStatusUpdate", "Failed", context.JobDetail.Key.Name, DateTime.Now);
        }
        finally
        {
            stopwatch.Stop();
            jobExecutionDetails.Duration = stopwatch.Elapsed;

            _context.Set<JobRunDetails>().Add(jobExecutionDetails);
            await _context.SaveChangesAsync();
        }
    }

    private string GetFullExceptionMessage(Exception ex)
    {
        var errorMessage = ex.Message;

        var innerException = ex.InnerException;
        while (innerException != null)
        {
            errorMessage += " --> " + innerException.Message;
            innerException = innerException.InnerException;
        }

        return errorMessage;
    }
}