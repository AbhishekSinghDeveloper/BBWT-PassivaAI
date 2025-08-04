using Serilog.Core;
using Serilog.Events;

namespace BBWM.Core.Loggers;

public class SourceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Source", AggregatedLogsSource.Server));
    }
}
