using BBWM.Core.Data;

namespace BBWM.AWS.EventBridge.Model;

public class EventBridgeJobHistory : IEntity
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public List<EventBridgeJobParameter> Parameters { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime FinishTime { get; set; }

    public JobCompletionStatus CompletionStatus { get; set; }

    public string ErrorMessage { get; set; }

    public string StackTrace { get; set; }
}
