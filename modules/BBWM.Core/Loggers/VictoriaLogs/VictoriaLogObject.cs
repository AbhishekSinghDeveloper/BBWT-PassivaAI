using Serilog.Events;

namespace BBWM.Core.Loggers.VictoriaLogs
{
    public class VictoriaLogObject
    {
        public LogEvent LogEvent { get;set;}
        public string ProjectName { get; set; }
        public string/*IReadOnlyDictionary<string, LogEventPropertyValue>*/ Properties { get; set; }

    }
}
