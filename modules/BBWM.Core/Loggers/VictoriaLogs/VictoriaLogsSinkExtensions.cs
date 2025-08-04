using Serilog.Configuration;
using Serilog;

namespace BBWM.Core.Loggers.VictoriaLogs
{
    public static class VictoriaLogsSinkExtensions
    {
        public static LoggerConfiguration VictoriaLogsSink(
             this LoggerSinkConfiguration loggerConfiguration,
             string projectName = "BBWT3",
             IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new VictoriaLogsSink(formatProvider, projectName));
        }
    }
}
