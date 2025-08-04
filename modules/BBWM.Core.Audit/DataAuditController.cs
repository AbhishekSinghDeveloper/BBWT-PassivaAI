using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

[Route("api/data-audit")]
[Authorize(Roles = Roles.SystemAdminRole)]
public class DataAuditController : Web.ControllerBase
{
    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage([FromQuery] QueryCommand command,
        [FromServices] IDataService<IAuditContext> dataService, CancellationToken ct = default)
        => Ok(await dataService.GetPage<ChangeLog, ChangeLogDTO>(command,
            (query) => query.Include(l => l.ChangeLogItems), ct));
}
