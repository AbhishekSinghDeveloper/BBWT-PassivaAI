using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

namespace BBWM.AWS.EventBridge.Api;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/aws-event-bridge-tech")]
[Authorize(Roles = Core.Roles.SystemAdminRole)]
public class AwsEventBridgeTechController : Core.Web.ControllerBase
{
    private readonly IAwsEventBridgeTechService techService;

    public AwsEventBridgeTechController(IAwsEventBridgeTechService techService)
    {
        this.techService = techService;
    }

    [HttpPut("clear-tables")]
    public IActionResult ClearTables()
        => Ok(techService.ClearEventBridgeTablesAsync());
}
