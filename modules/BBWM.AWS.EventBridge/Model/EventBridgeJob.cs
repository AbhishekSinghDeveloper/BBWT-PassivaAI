using BBWM.Core.Data;

namespace BBWM.AWS.EventBridge.Model;

public class EventBridgeJob : IEntity
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public List<EventBridgeJobParameter> Parameters { get; set; }

    public DateTime? LastExecutionTime { get; set; }

    public DateTime? NextExecutionTime { get; set; }

    public string TimeZone { get; set; }
}
