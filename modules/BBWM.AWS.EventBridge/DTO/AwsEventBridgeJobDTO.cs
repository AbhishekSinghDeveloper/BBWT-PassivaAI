using BBWM.Core.DTO;

namespace BBWM.AWS.EventBridge.DTO;

public class AwsEventBridgeJobDTO : IDTO
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public List<AwsEventBridgeJobParameterDTO> Parameters { get; set; }

    public DateTime? LastExecutionTime { get; set; }

    public DateTime? NextExecutionTime { get; set; }

    public string TimeZone { get; set; }
}
