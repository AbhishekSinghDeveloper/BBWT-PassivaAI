namespace BBWM.AggregatedLogs.Lambda.DTO;

public class EventDTO
{
    public AwsLogDTO awslogs { get; set; } = new AwsLogDTO();
}
