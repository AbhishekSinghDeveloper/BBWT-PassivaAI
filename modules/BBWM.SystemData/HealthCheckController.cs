using BBWM.SystemData.DTO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.SystemData;

[Route("api/health")]
public class HealthCheckController : ControllerBase
{
    private readonly ISystemDataService _systemDataService;

    public HealthCheckController(ISystemDataService systemDataService)
        => _systemDataService = systemDataService;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        var verInfo = _systemDataService.GetVersionInfo();

        var details = new HealthCheckDTO
        {
            ProductVersion = verInfo?.FullProductVersion,
            PipelineId = verInfo?.Pipeline
        };

        return Ok(details);
    }
}
