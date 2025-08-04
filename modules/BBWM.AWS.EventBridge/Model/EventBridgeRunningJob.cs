using BBWM.Core.Data;

namespace BBWM.AWS.EventBridge.Model;

public class EventBridgeRunningJob : IEntity
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public Guid CancelationId { get; set; } = Guid.Empty;

    public DateTime StartTime { get; set; }
}
