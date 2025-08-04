namespace BBWM.AWS.EventBridge;

internal class RuntimeJobRunningInfo
{
    public CancellationTokenSource TokenSource { get; set; }

    public JobCompletionStatus CompletionStatus { get; set; } = JobCompletionStatus.Unknown;
}
