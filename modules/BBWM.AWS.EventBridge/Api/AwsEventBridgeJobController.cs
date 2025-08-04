using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

namespace BBWM.AWS.EventBridge.Api;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/aws-event-bridge-job")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AwsEventBridgeJobController : Core.Web.ControllerBase
{
    private readonly IAwsEventBridgeJobService _awsEventBridgeJobService;

    public AwsEventBridgeJobController(IAwsEventBridgeJobService awsEventBridgeJobService)
        => _awsEventBridgeJobService = awsEventBridgeJobService;

    [HttpPost("restart-job/{historyId}")]
    public Task<IActionResult> RestartJobAsync(int historyId)
        => NoContent(() => _awsEventBridgeJobService.RestartJobAsync(historyId));


    [HttpGet("jobs-list")]
    public async Task<IActionResult> GetJobsListAsync(CancellationToken cancellationToken)
        => Ok(await _awsEventBridgeJobService.GetJobsListAsync(cancellationToken));
}
