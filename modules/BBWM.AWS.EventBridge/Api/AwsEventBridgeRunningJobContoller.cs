using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

namespace BBWM.AWS.EventBridge.Api;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/aws-event-bridge-running")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AwsEventBridgeRunningJobContoller : Core.Web.ControllerBase
{
    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage(
        [FromQuery] QueryCommand command,
        [FromServices] IDataService dataService,
        CancellationToken ct = default)
        => Ok(await dataService.GetPage<EventBridgeRunningJob, AwsEventBridgeRunningJobDTO>(command, ct));

    [HttpPut("cancel/{cancelationId}")]
    public IActionResult CancelJobAsync(Guid cancelationId)
        => NoContent(() => JobExecutionWrapper.CancelByUser(cancelationId));
}
