using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.AWS.EventBridge.Api;

[Produces("application/json")]
[Route("api/aws-event-bridge-job")]
[AuthorizeEventBridge]
public class AwsEventBridgeJobStarterController : Core.Web.ControllerBase
{
    private readonly IAwsEventBridgeJobService _awsEventBridgeJobService;

    public AwsEventBridgeJobStarterController(IAwsEventBridgeJobService awsEventBridgeJobService)
        => _awsEventBridgeJobService = awsEventBridgeJobService;

    [HttpPost("start-job/{ruleId}", Name = "StartJob")]
    [IgnoreAntiforgeryToken] // This endpoint is available only for Amazon requests so we make sure the
                             // request doesn't fail because of trying to validate the antiforgery token
    public Task<IActionResult> StartJobAsync(AwsEventBridgeStartJobDTO startInfo, CancellationToken ct = default)
        => NoContent(() => _awsEventBridgeJobService.StartJobAsync(startInfo, ct));
}
