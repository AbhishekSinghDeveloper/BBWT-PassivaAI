using BBWM.Core.DTO;

namespace BBWM.AWS.EventBridge.DTO;

public class AwsEventBridgeRunningJobDTO : IDTO
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public Guid CancelationId { get; set; }

    public DateTime StartTime { get; set; }
}
