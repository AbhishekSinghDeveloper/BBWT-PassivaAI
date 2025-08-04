using BBWM.Core.Filters;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Membership.Api;

[Route("api/login-audit")]
[Authorize(Roles = Core.Roles.SystemAdminRole)]
public class LoginAuditController : Web.ControllerBase
{
    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage([FromQuery] QueryCommand command,
        [FromServices] IDataService dataService, CancellationToken ct = default)
        => Ok(await dataService.GetPage<LoginAudit, LoginAuditDTO>(command, ct));
}
