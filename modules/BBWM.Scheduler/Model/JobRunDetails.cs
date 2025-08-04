using BBWM.Core.Data;
using System.ComponentModel.DataAnnotations;

namespace BBWM.Scheduler.Model;

public class JobRunDetails : IEntity
{
    public int Id { get; set; }
    public string JobName { get; set; }
    public DateTime ExecutionTime { get; set; }
    public bool Success { get; set; }

    [MaxLength(1000)]
    public string Message { get; set; }
    public string Status { get; set; } // e.g., Enqueued, Scheduled, etc.
    public string? JobGroup { get; set; }
    public DateTime LastModified { get; set; }
    public string? JobType { get; set; }
    public string? TriggerType { get; set; }
    public string? TriggerGroup { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ServerName { get; set; }
    public bool IsRecurring { get; set; }
    public int RetryCount { get; set; }
    public string? AssemblyName { get; set; }
    public string? Cron { get; set; }
}