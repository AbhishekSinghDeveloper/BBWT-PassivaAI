using Serilog.Core;
using Serilog.Events;

using System.Net;

namespace BBWM.Core.Loggers;

public class ServerNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Server", Dns.GetHostName()));
    }
}
