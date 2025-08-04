using Serilog.Core;
using Serilog.Events;

using System.Net;

namespace BBWM.Core.Loggers;

public class DockerContainerIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ContainerID", Dns.GetHostName()));
    }
}
