using BBWM.Core.Membership.Filters;
using BBWM.Menu.DTO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Menu;

/// <summary>
/// Controller to provide functionality for menus
/// </summary>
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _service;


    public MenuController(IMenuService service)
        => _service = service;


    [HttpGet]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet, Route("me")]
    [IgnoreSetup2FaCheck]
    [Authorize]
    public async Task<IActionResult> GetForCurrentUser(CancellationToken cancellationToken) =>
        Ok(await _service.GetForUser(User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public async Task<IActionResult> Create([FromBody] MenuDTO dto, CancellationToken cancellationToken) =>
        Ok(await _service.Create(dto, cancellationToken));

    [HttpPost, Route("update")]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public async Task<IActionResult> Update([FromBody] MenuDTO dto, CancellationToken cancellationToken) =>
        Ok(await _service.Update(dto, cancellationToken));

    [HttpDelete, Route("{id}")]
    [Authorize(Roles = Core.Roles.SuperAdminRole)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Ok(await _service.Delete(id, cancellationToken));
}
