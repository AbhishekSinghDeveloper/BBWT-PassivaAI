using BBWM.Core.Membership.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Core.Membership.Api;

[Route("api/permission")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
        => _permissionService = permissionService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default) =>
        Ok(await _permissionService.GetAll(cancellationToken));
}
