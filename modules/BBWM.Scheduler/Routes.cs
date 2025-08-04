using BBWM.Core.Web;

namespace BBWM.Scheduler;
public static class Routes
{
    public static readonly Route SchedulerDashboard = new("/app/scheduler/dashboard", "Scheduler Dashboard");
    public static readonly Route SchedulerJobs = new("/app/scheduler/jobs", "Scheduler Jobs");
    public static readonly Route SchedulerRecurringJobs = new("/app/scheduler/recurring-jobs", "Scheduler Recurring Jobs");
    public static readonly Route SchedulerRetries = new("/app/scheduler/retries", "Scheduler Retries");
    public static readonly Route SchedulerServers = new("/app/scheduler/servers", "Scheduler Servers");

}