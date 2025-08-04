namespace BBWM.AggregatedLogs;

public interface IWebServerLogsService
{
    Task Parse(CancellationToken ct = default);
}
