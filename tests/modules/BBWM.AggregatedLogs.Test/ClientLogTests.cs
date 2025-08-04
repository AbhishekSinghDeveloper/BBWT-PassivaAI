using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace BBWM.AggregatedLogs.Test;

public class ClientLogTests
{
    [Fact]
    public async void ClientLogController_Test()
    {
        using (TestCorrelator.CreateContext())
        using (var serilogLogger = new LoggerConfiguration().WriteTo.Sink(new TestCorrelatorSink()).Enrich.FromLogContext().CreateLogger())
        {
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<ClientLogController>();

            var controller = new ClientLogController(logger);

            var result = controller.SaveLog(new ClientLogDTO
            {
                HttpStatus = 401,
                ExceptionMessage = "Access denied",
                Path = "SomePath",
                StackTrace = "SomeStack",
            });

            var errorsLogged = TestCorrelator.GetLogEventsFromCurrentContext();

            Assert.Single(errorsLogged);

            var error = errorsLogged.First();
            Assert.Equal(LogEventLevel.Error, error.Level);
            Assert.Equal("Access denied SomeStack", error.Exception.ToString());
            Assert.Equal("{Path}: {ErrorMessage}", error.MessageTemplate.Text);
            Assert.Equal("\"SomePath\"", error.Properties["Path"].ToString());
            Assert.Equal("\"Access denied\"", error.Properties["ErrorMessage"].ToString());
            Assert.Equal("401", error.Properties["HttpStatus"].ToString());
            Assert.Equal("\"Client\"", error.Properties["Source"].ToString());
        }
    }
}

