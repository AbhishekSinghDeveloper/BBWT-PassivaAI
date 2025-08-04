using BBWM.Core.Audit;
using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BBWT.Server.Admin.Controllers;

[Route("api/data-audit")]
public class DataAuditController : BBWM.Core.Web.ControllerBase
{
    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage([FromQuery] QueryCommand command,
        [FromServices] IDataService dataService, CancellationToken ct = default)
        => Ok(await dataService.GetPage<ChangeLog, ChangeLogDTO>(command,
            (query) => query.Include(l => l.ChangeLogItems), ct));
}
