namespace BBWM.AWS.EventBridge;

public enum JobCompletionStatus
{
    Unknown = 0,
    Succeed,
    Failed,
    CanceledByUser,
    CanceledByShutdown
}
