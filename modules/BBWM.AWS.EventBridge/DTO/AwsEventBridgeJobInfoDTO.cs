namespace BBWM.AWS.EventBridge.DTO;

public class AwsEventBridgeJobInfoDTO
{
    public string JobId { get; set; }

    public bool Available { get; set; }

    public string JobDescription { get; set; }

    public List<JobParameterInfo> Parameters { get; set; }
}
