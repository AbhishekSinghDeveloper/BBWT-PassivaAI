using BBWM.Core.Loggers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ControllerBase = BBWM.Core.Web.ControllerBase;
using SerilogContext = Serilog.Context.LogContext;

namespace BBWM.AggregatedLogs;

[Produces("application/json")]
[Route("api/client-log")]
public class ClientLogController : ControllerBase
{
    private readonly ILogger<ClientLogController> _logger;

    public ClientLogController(ILogger<ClientLogController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult SaveLog([FromBody] ClientLogDTO errorLogDto)
    {
        using (SerilogContext.PushProperty("Source", AggregatedLogsSource.Client))
        using (SerilogContext.PushProperty("HttpStatus", errorLogDto.HttpStatus))
        {
            _logger.LogError(new ClientException(errorLogDto.ExceptionMessage, errorLogDto.StackTrace), "{Path}: {ErrorMessage}", errorLogDto.Path, errorLogDto.ExceptionMessage);
        }

        return NoContent();
    }
}
