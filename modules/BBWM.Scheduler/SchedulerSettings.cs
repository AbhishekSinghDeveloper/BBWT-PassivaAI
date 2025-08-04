namespace BBWM.Scheduler;

public static class SchedulerSettings
{
    public const int JobRetentionDays = 7;

    public static readonly DateTime JobCleanupCutoffDate = DateTime.Now.AddDays(-JobRetentionDays);
}