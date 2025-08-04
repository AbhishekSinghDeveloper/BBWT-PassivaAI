using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWT.Server.Admin.Controllers;

[Route("api/login-audit")]
public class LoginAuditController : ControllerBase
{
    [HttpGet, Route("{id}"), ResponseCache(NoStore = true)]
    public async Task<IActionResult> Get(int id,
        [FromServices] IDataService dataService,
        CancellationToken ct = default)
        => Ok(await dataService.Get<LoginAudit, LoginAuditDTO>(id, ct));
}
