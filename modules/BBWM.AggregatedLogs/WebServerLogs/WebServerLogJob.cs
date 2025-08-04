using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.AggregatedLogs;

public class WebServerLogJob : IEventBridgeJob
{
    public const string JOB_ID = "WebServerLogJob";
    public const string JOB_DESCRIPTION = "Job for parsing web server logs into aggregated logs in database";

    private readonly IWebServerLogsService _webServerLogsService;

    public WebServerLogJob(IWebServerLogsService webServerLogsService)
    {
        _webServerLogsService = webServerLogsService;
    }

    public string JobId => JOB_ID;

    public string JobDescription => JOB_DESCRIPTION;

    public List<JobParameterInfo> Parameters => new List<JobParameterInfo>();

    public Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct = default) =>
        _webServerLogsService.Parse(ct);
}
