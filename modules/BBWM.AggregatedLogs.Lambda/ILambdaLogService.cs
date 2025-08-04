using BBWM.AggregatedLogs.Lambda.DTO;

namespace BBWM.AggregatedLogs.Lambda
{
    public interface ILambdaLogService
    {
        Task ProcessLogs(EventDTO input);
    }
}
