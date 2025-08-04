using BBWM.Core.DTO;

namespace BBWM.AWS.EventBridge.DTO;

public class AwsEventBridgeRuleDTO : IDTO<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string TargetJobId { get; set; }
    public List<AwsEventBridgeJobParameterDTO> Parameters { get; set; }
    public string Cron { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public DateTime? NextExecutionTime { get; set; }
    public string TimeZoneId { get; set; }

    public string GetAwsCron()
        => string.IsNullOrEmpty(Cron) ? Cron : $"cron({Cron})";
}
