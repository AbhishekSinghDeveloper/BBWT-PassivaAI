using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace BBWM.AggregatedLogs.Test;

public class WebServerLogJobTests
{
    [Fact]
    public async void WebServerLogJob_Test()
    {
        var settings = new WebServerLogsSettings
        {
            AppName = "AppName",
            SourceName = "SourceName",
            FolderPath = @"SomePath",
        };

        var options = new Mock<IOptionsSnapshot<WebServerLogsSettings>>();
        options.SetupGet<WebServerLogsSettings>(x => x.Value).Returns(settings);

        var fileProvider = new Mock<IFileProvider>();
        var files = new string[]
        {
            "C:\\log1.txt",
            "C:\\log2.txt",
            "C:\\log3.txt",
            "C:\\log4.txt",
        };
        fileProvider.Setup<IEnumerable<string>>(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<DateTimeOffset>())).Returns(files);
        fileProvider.Setup<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log1.txt"))).Returns(new string[]
        {
            "::1 - user@something.com 31/May/2022:16:29:04 +0300] \"GET/api/getData/ HTTP/2\" 200 434",
        });
        fileProvider.Setup<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log2.txt"))).Returns(new string[]
        {
            "::1 - user@something.com [11/May/2022:16:29:04 +0130] \"GET/api/getData/ HTTP/2\" 200 434",
            "192:168:1:1 - - [31/May/2022:16:29:04 +0330] \"GET/api/getData/ HTTP/2\" 500 434",
        });
        fileProvider.Setup<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log3.txt"))).Throws<IOException>();


        using (TestCorrelator.CreateContext())
        using (var serilogLogger = new LoggerConfiguration().WriteTo.Sink(new TestCorrelatorSink()).Enrich.FromLogContext().CreateLogger())
        {
            var logger = new SerilogLoggerFactory(serilogLogger).CreateLogger<WebServerLogsService>();

            var contextOptions = new DbContextOptionsBuilder<LogContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot())
                .Options;

            var logContext = new LogContext(contextOptions);
            var lastLog = new Log
            {
                AppName = "AppName",
                Source = "SourceName",
                TimeStamp = new DateTimeOffset(2022, 05, 15, 16, 29, 04, TimeSpan.FromMinutes(90)),
            };
            logContext.Logs.Add(lastLog);
            logContext.SaveChanges();

            var service = new WebServerLogsService(options.Object, new NcsaParser(), logContext, fileProvider.Object, logger);

            var job = new WebServerLogJob(service);
            Assert.Equal("Job for parsing web server logs into aggregated logs in database", job.JobDescription);
            Assert.Equal("WebServerLogJob", job.JobId);
            Assert.Empty(job.Parameters);
            await job.RunAsync(null);

            fileProvider.Verify<IEnumerable<string>>(x => x.EnumerateFiles(It.Is<string>(i => i == settings.FolderPath), It.Is<string>(i => i == "*.*"), It.Is<SearchOption>(i => i == SearchOption.TopDirectoryOnly), It.Is<DateTimeOffset>(i => i == lastLog.TimeStamp)), Times.Once());
            fileProvider.Verify<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log1.txt")), Times.Once());
            fileProvider.Verify<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log2.txt")), Times.Once());
            fileProvider.Verify<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log3.txt")), Times.Once());
            fileProvider.Verify<IEnumerable<string>>(x => x.ReadLines(It.Is<string>(i => i == "C:\\log4.txt")), Times.Never());

            Assert.Equal(2, logContext.Logs.Count());

            Assert.Equal(500, logContext.Logs.Last().HttpStatus);

            var errorsLogged = TestCorrelator.GetLogEventsFromCurrentContext();
            Assert.Equal(2, errorsLogged.Count());

            var error = errorsLogged.First();
            Assert.Equal(LogEventLevel.Error, error.Level);
            Assert.Equal(@"Web Server logs parsing error, file C:\log1.txt: Invalid web server log line format: ::1 - user@something.com 31/May/2022:16:29:04 +0300] ""GET/api/getData/ HTTP/2"" 200 434", error.MessageTemplate.Text);

            error = errorsLogged.ElementAt(1);
            Assert.Equal(LogEventLevel.Error, error.Level);
            Assert.Equal(@"Web Server logs parsing error: I/O error occurred.", error.MessageTemplate.Text);
        }
    }
}

