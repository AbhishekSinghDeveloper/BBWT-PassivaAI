using BBWM.Core.Services;

namespace BBWM.AggregatedLogs;

public interface ILogService :
    IEntityQuery<Log>,
    IEntityPage<LogDTO>
{

}
