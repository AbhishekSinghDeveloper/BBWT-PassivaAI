# Quartz.Net Scheduler

## Abstract
The Schedule module (front-end accessible) allows us to manage jobs stored in our database. We can trigger, delete, and create new jobs from both the UI and backend. Jobs can be paused, resumed, and retried in case of failure. For CRON expressions, we utilize: [Quartz Scheduler Cron](https://www.quartz-scheduler.org/documentation/quartz-2.3.0/tutorials/crontrigger.html)
## Development and local testing
Quartz configuration is managed in the ***appsettings.development.json*** file. The database connection string is declared under ConnectionString. 
To configure Quartz, add the following to ***appsettings.development.json***: 
```json
    "SchedulerSettings": {
        "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"
    }
```
The transition from MySQL to MSSQL is managed through updates in the ***appsettings.json*** file. Specifically, the database connection settings are now configured under the ***DatabaseConnectionSettings*** section to use the MSSQL connection string
Tables are going to have prefix QRTZ_ , and, also, we have another table ***JobRunDetails***.

## Coding a sample job
To create a new job we need to implement the ***SchedulerConfigurator***. We need to add jobs at ***BBWT.Scheduler/Jobs/JobsName.cs***

Example:
```c#
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
```
Afterward, add the job in  ***SchedulerConfigurator*** under the ***ConfigureQuartzScheduler***  method, specifying the CRON schedule for the job:
```c#
  options
   .AddJob<TestReportingJob>(testReportingJobKey)
   .AddTrigger(trigger => trigger
       .ForJob(testReportingJobKey)
       .WithCronSchedule(cronExpression)
   );
```
## UI Usage
The UI for Quartz Scheduler consists of five pages:
### Scheduler Dashboard:
 Displays real-time and historical graphs, with weekly and daily views.
### Scheduler Jobs:
 Lists all jobs by status. Provides options to add, update, delete, pause, and resume jobs.
### Scheduler Retries:
 Tracks job retries and displays the number of attempts per job.
### Scheduler Recurring Jobs: 
Shows recurring jobs, allowing management from a dedicated table.
### Scheduler Servers: 
Displays details about the servers handling the jobs.
