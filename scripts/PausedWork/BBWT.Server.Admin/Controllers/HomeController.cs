using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWT.Server.Admin.Controllers;

[Produces("application/json")]
[Route("api/home")]
[Authorize]
public class HomeController : ControllerBase
{
    [HttpGet]
    [Route("auth-check")]
    public IActionResult Cap()
    {
        return NoContent();
    }
}
