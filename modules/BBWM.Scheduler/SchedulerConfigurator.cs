using BBWM.Core.Data;
using BBWM.Scheduler.Jobs;
using BBWM.Scheduler.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl.Matchers;

namespace BBWM.Scheduler;
public static class SchedulerConfigurator
{
    public static void ConfigureQuartzScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        // Select the appropriate connection string
        string connectionString;
        var isSqlServer = false;
        var connectionSettings = configuration["DatabaseConnectionSettings:DatabaseType"];

        switch (connectionSettings)
        {
            case "MsSql":
                connectionString = configuration.GetConnectionString("DefaultConnection");
                isSqlServer = true;
                break;
            case "MySql":
                connectionString = configuration.GetConnectionString("MySqlConnection");
                break;
            default: throw new InvalidOperationException($"Data base type '{connectionSettings}' is not supported.");
        }

        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();
            options.UsePersistentStore(s =>
            {
                if (isSqlServer)
                {
                    s.UseSqlServer(connectionString);
                }
                else
                {
                    s.UseMySql(connectionString);
                }

                s.UseJsonSerializer();
                s.PerformSchemaValidation = false;
            });

        var testReportingJobKey = JobKey.Create(nameof(TestReportingJob));
            var testReportingJobKey3 = JobKey.Create(nameof(TestReportingJob3));
            var jobCleanupJobKey = JobKey.Create(nameof(JobCleanupJob));

            var cronExpression = "0 0 0 * * ?"; // Every day at midnight

            options
             .AddJob<TestReportingJob>(testReportingJobKey)
             .AddTrigger(trigger => trigger
                 .ForJob(testReportingJobKey)
                 .WithCronSchedule(cronExpression)
             );

            options
            .AddJob<TestReportingJob3>(testReportingJobKey3)
            .AddTrigger(trigger => trigger
                .ForJob(testReportingJobKey3)
                .WithCronSchedule(cronExpression)
            );

            options
              .AddJob<JobCleanupJob>(jobCleanupJobKey)
              .AddTrigger(trigger => trigger
                  .ForJob(jobCleanupJobKey)
                  .WithCronSchedule("0 0 0 * * ?") // Every day at midnight
                 );
        });

        services.AddSingleton<IJobListener, JobCompletionListener>();
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        services.AddHostedService<QuartzManualJobStarter>();
        services.AddScoped<JobHelper>();
        services.AddScoped<QuartzSchemaInitializer>();
        services.AddHostedService<QuartzSchemaInitializerHostedService>();

        var serviceProvider = services.BuildServiceProvider();
    }

    public class QuartzManualJobStarter : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContext _dbContext;
        private readonly IHubContext<JobStatusHub> _hubContext;

        public QuartzManualJobStarter(IServiceProvider serviceProvider, IDbContext dbContext, IHubContext<JobStatusHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var scheduler = await scope.ServiceProvider.GetRequiredService<ISchedulerFactory>().GetScheduler();

            var jobKeys = new[] { new JobKey(nameof(TestReportingJob)), new JobKey(nameof(TestReportingJob3)) };

            foreach (var jobKey in jobKeys)
            {
                var jobExists = await scheduler.CheckExists(jobKey);
                if (!jobExists)
                {
                    var jobDetail = jobKey.Name == nameof(TestReportingJob)
                        ? JobBuilder.Create<TestReportingJob>().WithIdentity(jobKey).Build()
                        : JobBuilder.Create<TestReportingJob3>().WithIdentity(jobKey).Build();

                    var trigger = TriggerBuilder.Create()
                                                .WithIdentity($"{jobKey.Name}Trigger")
                                                .StartNow()
                                                .Build();

                    var jobExecutionDetails = new JobRunDetails
                    {
                        JobName = jobKey.Name,
                        JobGroup = jobKey.Group,
                        JobType = jobDetail.JobType.Name,
                        TriggerType = trigger.GetType().Name,
                        TriggerGroup = trigger.Key.Group,
                        ExecutionTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Status = "Enqueued",
                        Success = true,
                        Message = "Job enqueued for execution."
                    };
                    _dbContext.Set<JobRunDetails>().Add(jobExecutionDetails);
                    await _dbContext.SaveChangesAsync();

                    await scheduler.ScheduleJob(jobDetail, trigger);

                    await _hubContext.Clients.All.SendAsync("ReceiveJobStatusUpdate", "Enqueued", jobKey.Name, DateTime.Now);
                }
                bool isRecurring = await JobHelper.IsRecurringJob(scheduler, jobKey);
                Console.WriteLine($"The job '{jobKey.Name}' is {(isRecurring ? "" : "not ")}a recurring job.");
            }

            await scheduler.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public static async Task ReconfigureScheduler(this IServiceProvider serviceProvider)
    {
        var schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        await DeleteAllJobs(scheduler);

        var testReportingJobKey = JobKey.Create(nameof(TestReportingJob));

        var cronExpression = GetCronExpression(new ReportSchedule
        {
            Day = "WED",
            Time = "03:24"
        });

        var testJobDetail = JobBuilder.Create<TestReportingJob>()
            .WithIdentity(testReportingJobKey)
            .Build();

        var testTrigger = TriggerBuilder.Create()
            .ForJob(testReportingJobKey)
            .WithCronSchedule(cronExpression)
            .Build();

        await scheduler.ScheduleJob(testJobDetail, testTrigger);
    }

    private static async Task DeleteAllJobs(IScheduler scheduler)
    {
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        await scheduler.DeleteJobs(jobKeys);
    }

    private static string GetCronExpression(ReportSchedule schedule)
    {
        var dayOfWeek = schedule.Day switch
        {
            "Sunday" => "SUN",
            "Monday" => "MON",
            "Tuesday" => "TUE",
            "Wednesday" => "WED",
            "Thursday" => "THU",
            "Friday" => "FRI",
            "Saturday" => "SAT",
            _ => throw new ArgumentException("Invalid day of the week")
        };

        var timeParts = schedule.Time.Split(':');
        if (timeParts.Length != 2 || !int.TryParse(timeParts[0], out var hour) || !int.TryParse(timeParts[1], out var minute))
        {
            throw new ArgumentException("Invalid time format");
        }

        return $"0 {minute} {hour} ? * {dayOfWeek} *";
    }
}

public class QuartzSchemaInitializerHostedService : IHostedService
{
    private readonly QuartzSchemaInitializer _initializer;
    private readonly IHostApplicationLifetime _lifetime;

    public QuartzSchemaInitializerHostedService(QuartzSchemaInitializer initializer, IHostApplicationLifetime lifetime)
    {
        _initializer = initializer;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class QuartzSchemaInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}