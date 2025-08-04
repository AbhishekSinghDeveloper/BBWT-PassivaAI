using BBWM.Core.Membership.Filters;
using BBWM.Core.Membership.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using ClaimTypes = System.Security.Claims.ClaimTypes;
using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Core.Membership.Api;

[Produces("application/json")]
[Route("api/route")]
public class RouteController : ControllerBase
{
    private readonly IRouteRolesService _routesService;

    public RouteController(IRouteRolesService routesService)
    {
        _routesService = routesService;
    }

    [HttpGet, Route("api-routes-roles")]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public IActionResult GetRoutesRoles() =>
        Ok(_routesService.GetApiRoutesRoles());

    [HttpGet, Route("pages-roles")]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public IActionResult GetPageRoles() =>
        Ok(_routesService.GetPagesRoutes());

    [HttpGet, Route("me")]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> GetPageRoutesPathsForRolesAndGroups(CancellationToken cancellationToken = default) =>
        Ok(await _routesService.GetPageRoutesForUser(User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken));
}
