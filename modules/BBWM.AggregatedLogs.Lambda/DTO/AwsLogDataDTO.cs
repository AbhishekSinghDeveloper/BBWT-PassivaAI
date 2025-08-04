namespace BBWM.AggregatedLogs.Lambda.DTO;

internal class AwsLogDataDTO
{
    public string Owner { get; set; }

    public string LogGroup { get; set; }

    public string LogStream { get; set; }

    public List<String> SubscriptionFilters { get; set; } = new List<string>();

    public string MessageType { get; set; }

    public List<LogEntryDTO> LogEvents { get; set; } = new List<LogEntryDTO>();
}

