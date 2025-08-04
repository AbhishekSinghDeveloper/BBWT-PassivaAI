using BBWM.Core.DTO;

namespace BBWM.AWS.EventBridge.DTO;

public class AwsEventBridgeJobHistoryDTO : IDTO
{
    public int Id { get; set; }

    public string RuleId { get; set; }

    public string JobId { get; set; }

    public List<AwsEventBridgeJobParameterDTO> Parameters { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime FinishTime { get; set; }

    public JobCompletionStatus CompletionStatus { get; set; }

    public string ErrorMessage { get; set; }

    public string StackTrace { get; set; }
}
