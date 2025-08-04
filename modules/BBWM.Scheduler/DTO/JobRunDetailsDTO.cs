namespace BBWM.Scheduler.DTO;

public class JobRunDetailsDTO
{
    public int Id { get; set; }
    public string? JobName { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? MinutesSinceLastModified { get; set; }
    public string? Status {  get; set; }
    public string? Message { get; set; }
    public string? TimeUntilDeletion { get; set; }
    public string? ServerName { get; set; }
    public bool? IsRecurring { get; set; }
    public int RetryCount { get; set; }
    public string? Cron { get; set; }
    public string? ExecutionTime { get; set; }
}

