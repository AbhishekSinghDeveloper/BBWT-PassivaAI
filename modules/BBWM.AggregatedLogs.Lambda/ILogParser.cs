using BBWM.AggregatedLogs.Lambda.DTO;

namespace BBWM.AggregatedLogs.Lambda
{
    public interface ILogParser
    {
        Task<IEnumerable<Log>> Parse(EventDTO logEvent, CancellationToken ct = default);
    }
}
