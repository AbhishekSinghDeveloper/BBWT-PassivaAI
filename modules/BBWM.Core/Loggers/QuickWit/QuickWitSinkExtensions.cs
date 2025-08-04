using Serilog.Configuration;
using Serilog;
using BBWM.Core.Loggers.QuickWit;
using Microsoft.AspNetCore.Hosting;

namespace BBWM.Core.Loggers.VictoriaLogs
{
    public static class QuickWitSinkExtensions
    {
        public static LoggerConfiguration QuickWitSink(
             this LoggerSinkConfiguration loggerConfiguration,
        string projectName = "BBWT3",
        QuickWitOptions options = null)
        {
            return loggerConfiguration.Sink(new QuickWitSink(options, projectName));
        }
    }
}
