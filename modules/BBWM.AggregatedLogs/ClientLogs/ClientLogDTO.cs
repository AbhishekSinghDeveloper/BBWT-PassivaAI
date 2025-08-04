namespace BBWM.AggregatedLogs;

public class ClientLogDTO
{
    public int? HttpStatus { get; set; }
    public string ExceptionMessage { get; set; }
    public string StackTrace { get; set; }
    public string Path { get; set; }
}
