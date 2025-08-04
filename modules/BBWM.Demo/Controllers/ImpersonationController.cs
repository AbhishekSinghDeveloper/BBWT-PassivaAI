using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Interfaces;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Demo.Controllers;

[Route("api/demo/impersonation")]
public class ImpersonationController : ControllerBase
{
    private readonly IUserDataService _userDataService;


    public ImpersonationController(IUserDataService userDataService)
        => _userDataService = userDataService;


    [HttpGet]
    [Route("impersonated-demo-manager")]
    public async Task<IActionResult> GetDemoManagerForImpersonation()
    {
        var user = await _userDataService.GetByEmail(InitialUsers.Manager.Email);

        if (user is null)
            throw new EntityNotFoundException();

        return Ok(user);
    }

    [HttpGet]
    [Route("impersonated-demo-user")]
    public async Task<IActionResult> GetDemoUserForImpersonation()
    {
        var user = await _userDataService.GetByEmail(InitialUsers.DemoUser.Email);

        if (user is null)
            throw new EntityNotFoundException();

        return Ok(user);
    }
}
