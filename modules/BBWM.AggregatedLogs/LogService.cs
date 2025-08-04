using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Services;

namespace BBWM.AggregatedLogs;

public class LogService : ILogService
{
    private readonly IDataService<ILogContext> dataService;

    public LogService(IDataService<ILogContext> dataService) => this.dataService = dataService;

    public IQueryable<Log> GetEntityQuery(IQueryable<Log> baseQuery) => baseQuery;

    public Task<PageResult<LogDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        => dataService.GetPage<Log, LogDTO>(command, GetEntityQuery,
            queryFilter => queryFilter
                .Handle<NumberFilter>("timeWindow", SetTimeWindow),
            ct: ct);

    private static IQueryable<Log> SetTimeWindow(IQueryable<Log> query, NumberFilter filter)
    {
        return (TimeWindows)filter.Value switch
        {
            TimeWindows.TenMinutes => query.Where(x => x.TimeStamp.AddMinutes(10) > DateTime.Now),
            TimeWindows.ThirtyMinutes => query.Where(x => x.TimeStamp.AddMinutes(30) > DateTime.Now),
            TimeWindows.OneHour => query.Where(x => x.TimeStamp.AddHours(1) > DateTime.Now),
            TimeWindows.ThreeHours => query.Where(x => x.TimeStamp.AddHours(3) > DateTime.Now),
            TimeWindows.TwelveHours => query.Where(x => x.TimeStamp.AddHours(12) > DateTime.Now),
            _ => query
        };
    }
}