using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.SystemData;

[Route("api/system-data")]
public class SystemDataController : ControllerBase
{
    private readonly ISystemDataService _systemDataService;

    public SystemDataController(ISystemDataService systemDataService)
        => _systemDataService = systemDataService;

    [Route("summary")]
    [HttpGet]
    [Authorize(Roles = Core.Roles.SuperAdminRole + "," + Core.Roles.SystemAdminRole)]
    public async Task<IActionResult> GetSystemSummary()
        => Ok(await _systemDataService.GetSystemSummary());

    [Route("version")]
    [HttpGet]
    [Authorize(Roles = Core.Roles.SuperAdminRole + "," + Core.Roles.SystemAdminRole)]
    public IActionResult GetVersionInfo()
        => Ok(_systemDataService.GetVersionInfo());

    [Route("debug")]
    [HttpGet]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public IActionResult GetDebugInfo()
        => Ok(_systemDataService.GetDebugInfo());

    [Route("debug/api-exeptions")]
    [HttpGet]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public IActionResult GetApiExceptionsInfo()
        => Ok(_systemDataService.GetApiExceptions());

    /// <summary>
    /// TODO: explain two purposes: 1) site refreshing in runtime; 2) prolonging user session
    /// </summary>
    /// <returns>Pipeline ID used as the back-end API version</returns>
    [Route("renew")]
    [HttpGet]
    [Authorize]
    public IActionResult Renew()
        => Ok(_systemDataService.GetVersionInfo()?.Pipeline ?? (DateTime.Now.Millisecond < 100 ? "1" : "0"));
}