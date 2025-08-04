using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Filters;
using BBWM.Core.Web.Filters;
using BBWM.Menu.DTO;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Menu;

[Route("api/footer-menu")]
[ReadWriteAuthorize(ReadRoles = Core.Roles.SuperAdminRole, WriteRoles = Core.Roles.SuperAdminRole)]
public class FooterMenuController : ControllerBase
{
    private readonly IFooterMenuService _footerMenuService;


    public FooterMenuController(IFooterMenuService footerMenuItemService)
        => _footerMenuService = footerMenuItemService;

    [HttpGet]
    [Authorize]
    [IgnoreSetup2FaCheck]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _footerMenuService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] FooterMenuItemDTO dto, CancellationToken cancellationToken) =>
        Ok(await _footerMenuService.Save(dto, cancellationToken));

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromBody] FooterMenuItemDTO dto, CancellationToken cancellationToken) =>
        Ok(await _footerMenuService.Save(dto, cancellationToken));

    [HttpPost("update-order")]
    public Task<IActionResult> UpdateOrderOfItems([FromBody] IEnumerable<FooterMenuItemDTO> items,
            CancellationToken cancellationToken)
        => NoContent(() => _footerMenuService.UpdateOrderOfItems(items, cancellationToken));

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => NoContent(async () =>
        {
            if (!await _footerMenuService.Exists(id, cancellationToken))
                throw new EntityNotFoundException("Footer menu item not found.");
            await _footerMenuService.Delete(id, cancellationToken);
        });
}
