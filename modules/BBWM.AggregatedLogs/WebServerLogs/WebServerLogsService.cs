using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace BBWM.AggregatedLogs;

public class WebServerLogsService : IWebServerLogsService
{
    private readonly WebServerLogsSettings _settings;
    private readonly ILogLineParser _lineParser;
    private readonly ILogContext _logContext;
    private readonly ILogger<WebServerLogsService> _logger;
    private readonly IFileProvider _fileProvider;

    public WebServerLogsService(IOptionsSnapshot<WebServerLogsSettings> settings, ILogLineParser lineParser, ILogContext logContext, IFileProvider fileProvider, ILogger<WebServerLogsService> logger)
    {
        _settings = settings.Value;
        _lineParser = lineParser;
        _logContext = logContext;
        _logger = logger;
        _fileProvider = fileProvider;
    }

    public async Task Parse(CancellationToken ct = default)
    {
        try
        {
            var lastTimestamp = await _logContext.Logs
                .Where(l => l.AppName == _settings.AppName && l.Source == _settings.SourceName)
                .Select(l => l.TimeStamp)
                .OrderByDescending(l => l)
                .FirstOrDefaultAsync(ct);

            var serverName = Dns.GetHostName();

            var files = _fileProvider.EnumerateFiles(_settings.FolderPath, "*.*", SearchOption.TopDirectoryOnly, lastTimestamp);
            foreach (var file in files)
            {
                if (ct.IsCancellationRequested)
                    break;

                foreach (string line in _fileProvider.ReadLines(file))
                {
                    if (ct.IsCancellationRequested)
                        break;

                    Log log = null;

                    try
                    {
                        log = _lineParser.Parse(line, _settings.AppName, serverName, _settings.SourceName);

                    }
                    catch (InvalidFormatException ex)
                    {
                        _logger.LogError(ex, $"Web Server logs parsing error, file {file}: {ex.Message}");
                        break;  // wrong format -> skip the whole file
                    }

                    if (log.TimeStamp > lastTimestamp)
                    {
                        _logContext.Logs.Add(log);
                    }
                }

                await _logContext.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Web Server logs parsing error: {ex.Message}");
        }
    }
}
