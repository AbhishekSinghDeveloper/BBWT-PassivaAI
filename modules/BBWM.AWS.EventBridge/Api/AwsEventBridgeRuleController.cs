using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.AWS.EventBridge.Api;

[Produces("application/json")]
[Route("api/aws-event-bridge-rule")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AwsEventBridgeRuleController : DataControllerBase<IEntity<string>, AwsEventBridgeRuleDTO, AwsEventBridgeRuleDTO, string>
{
    private readonly IAwsEventBridgeRuleService service;

    public AwsEventBridgeRuleController(IAwsEventBridgeRuleService service)
        : base(service)
        => this.service = service;

    [HttpGet("exists/{name}")]
    public async Task<IActionResult> RuleExistsAsync(string name, CancellationToken cancellationToken = default)
        => Ok(await service.RuleExistsAsync(name, cancellationToken));
}
