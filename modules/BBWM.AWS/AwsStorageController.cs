using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.AWS;

[Produces("application/json")]
[Route("api/aws-storage")]
public class AwsStorageController : ControllerBase
{
    private readonly IAwsService _awsService;


    public AwsStorageController(IAwsService awsService)
        => _awsService = awsService;


    [HttpGet, Route("permissions-check")]
    public async Task<IActionResult> CheckPermissions() =>
        Ok(await _awsService.CheckPermissions());
}
