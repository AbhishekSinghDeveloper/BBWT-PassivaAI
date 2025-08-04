using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Scheduler.DTO;
using BBWM.Scheduler.Jobs;
using BBWM.Scheduler.Model;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl;
using System.Reflection;

namespace BBWM.Scheduler.Service;

public class SchedulerJobService : ISchedulerJobService
{
    private readonly IDbContext _dbContext;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IScheduler _scheduler;

    public SchedulerJobService(IDbContext dbContext, ISchedulerFactory schedulerFactory)
    {
        _dbContext = dbContext;
        _schedulerFactory = schedulerFactory;
    }

    public async Task<IEnumerable<object>> GetOverviewAsync(string? view, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var startTime = now.AddHours(-24);

        return view switch
        {
            "day" => await GetDailyOverviewAsync(startTime, ct),
            "week" => await GetWeeklyOverviewAsync(now, ct),
            _ => Enumerable.Empty<object>()
        };
    }

    private async Task<IEnumerable<Dictionary<string, int>>> GetDailyOverviewAsync(DateTime startTime, CancellationToken ct)
    {
        var hourlyData = new List<Dictionary<string, int>>();
        for (int hour = 0; hour < 24; hour++)
        {
            var startHour = startTime.AddHours(hour);
            var endHour = startHour.AddHours(1);
            var data = await GetJobStatusCountsAsync(startHour, endHour, ct);
            hourlyData.Add(data);
        }
        return hourlyData;
    }

    private async Task<Dictionary<string, int>> GetJobStatusCountsAsync(DateTime start, DateTime end, CancellationToken ct)
    {
        return new Dictionary<string, int>
            {
                { "Failed", await _dbContext.Set<JobRunDetails>().CountAsync(j => j.Status == "Failed" && j.LastModified >= start && j.LastModified < end, ct) },
                { "Deleted", await _dbContext.Set<JobRunDetails>().CountAsync(j => j.Status == "Deleted" && j.LastModified >= start && j.LastModified < end, ct) },
                { "Succeeded", await _dbContext.Set<JobRunDetails>().CountAsync(j => j.Status == "Succeeded" && j.LastModified >= start && j.LastModified < end, ct) }
            };
    }

    private async Task<IEnumerable<DailyJobDTO>> GetWeeklyOverviewAsync(DateTime endOfWeek, CancellationToken ct)
    {
        var weeklyData = new List<DailyJobDTO>();
        for (int day = 0; day < 7; day++)
        {
            var currentDay = endOfWeek.AddDays(-day);
            var nextDay = currentDay.AddDays(1);
            var data = await GetJobStatusCountsAsync(currentDay, nextDay, ct);
            weeklyData.Add(new DailyJobDTO
            {
                Date = currentDay,
                Failed = data["Failed"],
                Deleted = data["Deleted"],
                Succeeded = data["Succeeded"]
            });
        }
        weeklyData.Reverse();
        return weeklyData;
    }

    public async Task<PageResultDTO> GetJobsByStatusAsync(string status, QueryCommand command = null, CancellationToken ct = default)
    {
        try
        {
            var query = _dbContext.Set<JobRunDetails>().AsQueryable();

            if (status == "Awaiting")
            {
                query = query.Where(j => j.Status == status || j.Status == "Paused");
            }
            else
            {
                query = query.Where(j => j.Status == status);
            }

            var groupedQuery = query
                .Where(x => x.Cron != null)
                .GroupBy(j => new { j.JobName, j.Cron })
                .Select(g => g.OrderByDescending(j => j.LastModified).FirstOrDefault());

            return await GetFilteredJobsResultAsync(groupedQuery, command, ct);
        }
        catch (Exception ex)
        {
            throw new BusinessException("Cannot get query source service.", ex);
        }
    }

    public async Task<JobRunDetailsDTO> GetJobDetailsAsync(int jobId, CancellationToken ct = default)
    {
        try
        {
            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.Id == jobId, ct);

            if (job == null) throw new BusinessException($"Job with Id {jobId} not found.");

            var jobCleanupCutoffDate = DateTime.Now.AddDays(-SchedulerSettings.JobRetentionDays);
            TimeSpan timeRemaining = job.LastModified.AddDays(SchedulerSettings.JobRetentionDays) - DateTime.Now;

            return new JobRunDetailsDTO
            {
                Id = job.Id,
                JobName = job.JobName,
                LastModified = job.LastModified,
                Status = job.Status,
                Message = job.Message,
                TimeUntilDeletion = FormatTimeUntilDeletion(timeRemaining)
            };
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Job with Id {jobId} not found.", ex);
        }
    }

    public async Task<PageResultDTO> GetRecurringJobsAsync(QueryCommand command = null, CancellationToken ct = default)
    {
        var recurringJobsQuery = _dbContext.Set<JobRunDetails>()
                                           .Where(j => j.IsRecurring == true);

        var groupedQuery = recurringJobsQuery
           .Where(x => x.Cron != null)
           .GroupBy(j => new { j.JobName, j.Cron })
           .Select(g => g.OrderByDescending(j => j.LastModified).FirstOrDefault());

        return await GetFilteredJobsResultAsync(groupedQuery, command, ct);
    }

    public async Task<PageResultDTO> GetRetriedJobsAsync(QueryCommand command = null, CancellationToken ct = default)
    {
        var retriedJobsQuery = _dbContext.Set<JobRunDetails>()
             .Where(j => j.RetryCount > 0);

        var groupedQuery = retriedJobsQuery
           .Where(x => x.Cron != null)
           .GroupBy(j => new { j.JobName, j.Cron })
           .Select(g => g.OrderByDescending(j => j.LastModified).FirstOrDefault());

        return await GetFilteredJobsResultAsync(groupedQuery, command, ct);
    }

    public async Task<PageServerResultDTO> GetServersAsync(QueryCommand command = null, CancellationToken ct = default)
    {
        var serverQuery = _dbContext.Set<JobRunDetails>()
            .GroupBy(j => j.ServerName)
            .Select(g => new
            {
                ServerName = g.Key,
                Workers = g.Count(),
                Queues = string.Join(", ", g.Select(j => j.TriggerGroup).Distinct()),
                Started = g.Min(j => j.ExecutionTime),
                Heartbeat = g.Max(j => j.LastModified)
            });

        var total = serverQuery.Count();

        if (command?.Skip is not null)
            serverQuery = serverQuery.Skip(command.Skip.Value);
        if (command?.Take is not null)
            serverQuery = serverQuery.Take(command.Take.Value);

        var servers = await serverQuery.ToListAsync(ct);

        var now = DateTime.UtcNow;

        var serverInfoList = servers.Select(s => new ServerInfoDTO
        {
            ServerName = s.ServerName,
            Workers = s.Workers,
            Queues = s.Queues,
            Started = s.Started,
            Heartbeat = s.Heartbeat,
            StartedFormatted = FormatTimeDifference(now - s.Started),
            HeartbeatFormatted = FormatTimeDifference(now - s.Heartbeat)
        }).ToList();

        return new PageServerResultDTO
        {
            Items = serverInfoList,
            Total = total
        };
    }

    public async Task<bool> TriggerJobAsync(int jobId, string message = null, CancellationToken ct = default)
    {
        try
        {

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.Id == jobId, ct);
            if (job == null) throw new BusinessException("No job found.");
            ;

            var jobDataMap = new JobDataMap
            {
                { "jobId", job.Id },
                { "jobName", job.JobName },
                { "lastModified", job.LastModified.ToString("yyyy-MM-dd HH:mm:ss.fffffff") },
                { "status", "Succeeded" },
            };

            job.Status = "Succeeded";
            job.LastModified = DateTime.Now;
            if (message != null)
            {
                job.Message = message;
            }
            else
            {
                job.Message = "Job executed successfully";
            }
            _dbContext.Entry(job).Property(j => j.Status).IsModified = true;
            await _dbContext.SaveChangesAsync(ct);

            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKey = new JobKey(job.JobName, job.JobGroup);
            if (await scheduler.CheckExists(jobKey, ct))
            {
                await scheduler.TriggerJob(jobKey, jobDataMap, ct);
            }
            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException("Error triggering jobs.", ex);

        }
    }

    public async Task<bool> PauseJobAsync(int jobId, CancellationToken ct = default)
    {
        try
        {

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.Id == jobId, ct);
            if (job == null) throw new BusinessException("No job was found.");

            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKey = new JobKey(job.JobName, job.JobGroup);

            await scheduler.PauseJob(jobKey, ct);

            job.Status = "Awaiting";
            job.LastModified = DateTime.Now;
            _dbContext.Entry(job).Property(j => j.Status).IsModified = true;
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException("Error pausing job.", ex);
        }
    }

    public async Task<bool> ResumeJobAsync(int jobId, CancellationToken ct = default)
    {
        try
        {

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.Id == jobId, ct);
            if (job == null) throw new BusinessException("No job was found.");

            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKey = new JobKey(job.JobName, job.JobGroup);

            if (!await scheduler.CheckExists(jobKey, ct))
            {
                var jobDetail = JobBuilder.Create<TestReportingJob>()
                                           .WithIdentity(jobKey)
                                           .Build();

                var trigger = TriggerBuilder.Create()
                                            .WithIdentity($"{job.JobName}Trigger", job.TriggerGroup)
                                            .StartNow()
                                            .Build();

                await scheduler.ScheduleJob(jobDetail, trigger, ct);
            }

            job.Status = "Scheduled";
            job.LastModified = DateTime.Now;
            _dbContext.Entry(job).Property(j => j.Status).IsModified = true;
            await _dbContext.SaveChangesAsync(ct);

            await scheduler.ResumeJob(jobKey, ct);

            await TriggerJobAsync(jobId, null, ct);

            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException("Error resuming job.", ex);
        }
    }

    public async Task<bool> RetryJobAsync(int jobId, CancellationToken ct = default)
    {
        try
        {

            var job = await _dbContext.Set<JobRunDetails>().FirstOrDefaultAsync(j => j.Id == jobId, ct);
            if (job == null) throw new BusinessException("No job was found.");

            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKey = new JobKey(job.JobName, job.JobGroup);

            if (!await scheduler.CheckExists(jobKey, ct))
            {
                var jobDetail = JobBuilder.Create<TestReportingJob>()
                                          .WithIdentity(jobKey)
                                          .Build();

                var trigger = TriggerBuilder.Create()
                                            .WithIdentity($"{job.JobName}Trigger", job.TriggerGroup)
                                            .StartNow()
                                            .Build();

                await scheduler.ScheduleJob(jobDetail, trigger, ct);
            }
            else
            {
                await scheduler.ResumeJob(jobKey, ct);
            }

            job.Status = "Scheduled";
            job.LastModified = DateTime.Now;
            _dbContext.Entry(job).Property(j => j.Status).IsModified = true;
            await _dbContext.SaveChangesAsync(ct);

            await TriggerJobAsync(jobId, null, ct);

            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException("Error retrying job.", ex);
        }
    }

    public async Task<bool> DeleteJobAsync(int jobId, CancellationToken ct = default)
    {
        try
        {
            var jobDetails = await _dbContext.Set<JobRunDetails>()
                .Where(j => j.Id == jobId)
                .Select(j => new { j.JobName, j.Cron, j.JobGroup })
                .FirstOrDefaultAsync(ct);

            if (jobDetails == null) throw new BusinessException("No job was found.");

            var scheduler = await _schedulerFactory.GetScheduler(ct);
            var jobKey = new JobKey(jobDetails.JobName, jobDetails.JobGroup);
            var triggers = await scheduler.GetTriggersOfJob(jobKey, ct);
            var triggerToDelete = triggers.FirstOrDefault(t => t is ICronTrigger cronTrigger && cronTrigger.CronExpressionString == jobDetails.Cron);

            if (triggerToDelete != null)
            {
                await scheduler.UnscheduleJob(triggerToDelete.Key, ct);
            }

            var remainingTriggers = await scheduler.GetTriggersOfJob(jobKey, ct);
            if (!remainingTriggers.Any())
            {
                var jobDeleted = await scheduler.DeleteJob(jobKey, ct);
                if (!jobDeleted)
                {
                    throw new BusinessException("Job could not be deleted from the scheduler.");
                }
            }

            var jobsToDelete = await _dbContext.Set<JobRunDetails>()
                .Where(j => j.JobName == jobDetails.JobName && j.Cron == jobDetails.Cron)
                .ToListAsync(ct);

            if (jobsToDelete.Count == 0)
            {
                throw new BusinessException("No jobs were found with the same name and cron expression.");
            }

            _dbContext.Set<JobRunDetails>().RemoveRange(jobsToDelete);
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            throw new BusinessException("Error deleting job.", ex);
        }
    }

    private string FormatTimeDifference(TimeSpan timeDifference)
    {
        if (timeDifference.TotalDays >= 1)
            return $"{(int)timeDifference.TotalDays} days ago";
        if (timeDifference.TotalHours >= 1)
            return $"{(int)timeDifference.TotalHours} hours ago";
        if (timeDifference.TotalMinutes >= 1)
            return $"{(int)timeDifference.TotalMinutes} minutes ago";
        return "just now";
    }

    private string FormatTimeUntilDeletion(TimeSpan timeRemaining)
    {
        if (timeRemaining.TotalHours < 1)
            return $"{(int)timeRemaining.TotalMinutes} minutes";

        if (timeRemaining.TotalDays < 1)
            return $"{(int)timeRemaining.TotalHours} hours";

        return $"{(int)timeRemaining.TotalDays} days and {timeRemaining.Hours} hours";
    }

    private async Task<PageResultDTO> GetFilteredJobsResultAsync(IQueryable<JobRunDetails> groupedQuery, QueryCommand command, CancellationToken ct)
    {
        var total = await groupedQuery.CountAsync(ct);
        var filterQuery = ApplySorting(groupedQuery, command);

        if (command?.Skip is not null)
            filterQuery = filterQuery.Skip(command.Skip.Value).ToList();

        if (command?.Take is not null)
            filterQuery = filterQuery.Take(command.Take.Value).ToList();

        DateTime now = DateTime.Now;

        var items = filterQuery.Select(item => new JobRunDetailsDTO
        {
            Id = item.Id,
            JobName = item.JobName,
            LastModified = item.LastModified,
            MinutesSinceLastModified = FormatTimeDifference(now - item.LastModified),
            Status = item.Status,
            RetryCount = item.RetryCount,
            ExecutionTime = FormatTimeDifference(now - item.ExecutionTime),
            Cron = item.Cron
        }).ToList();

        return new PageResultDTO
        {
            Items = items,
            Total = total
        };
    }

    private List<JobRunDetails> ApplySorting(IQueryable<JobRunDetails> query, QueryCommand command)
    {
        var filterQuery = new List<JobRunDetails>();

        if (!string.IsNullOrEmpty(command?.SortingField))
        {
            if (command.SortingField == "minutesSinceLastModified")
            {
                command.SortingField = "LastModified";
            }

            var propertyInfo = typeof(JobRunDetails).GetProperty(command.SortingField,
                 BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                if (command.SortingDirection == OrderDirection.Desc)
                    filterQuery = query.OrderByDescending(propertyInfo.GetValue).ToList();
                else
                    filterQuery = query.OrderBy(propertyInfo.GetValue).ToList();
            }
        }

        return filterQuery;
    }

    public async Task<bool> RuleExistsAsync(string name, CancellationToken ct = default)
    {
        try
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            var jobChecker = await DoesJobExistAsync(scheduler.SchedulerName);

            return jobChecker;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DoesJobExistAsync(string jobName)
    {
        var jobKey = new JobKey(jobName);
        var jobDetail = await _scheduler.GetJobDetail(jobKey);
        return jobDetail != null;
    }

    public async Task<bool> SaveJobAsync(string jobName, string cronExpression, CancellationToken ct = default)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.Start();

            JobKey jobKey = new JobKey(jobName);
            if (await scheduler.CheckExists(jobKey))
            {
                Console.WriteLine($"Job with key {jobName} already exists.");
                return false;
            }

            if (!scheduler.IsStarted)
            {
                await scheduler.Start();
            }

            IJobDetail jobDetail = JobBuilder.Create<MyJob>()
           .WithIdentity(jobName)
           .StoreDurably()
           .RequestRecovery()
           .Build();


            Console.WriteLine($"Job created with identity: {jobName}");

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}Trigger")
                .WithCronSchedule(cronExpression, x => x.WithMisfireHandlingInstructionFireAndProceed())
                .ForJob(jobDetail)
                .Build();

            Console.WriteLine($"Trigger created with cron expression: {cronExpression}");

            await scheduler.ScheduleJob(jobDetail, trigger, ct);
            Console.WriteLine($"Job {jobName} scheduled successfully");

            return true;
        }
        catch (SchedulerException ex)
        {
            Console.WriteLine($"SchedulerException occurred: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving job: {ex.Message}");
            return false;
        }
    }
}