using BBWM.Core.Web.Filters;
using BBWM.RuntimeEditor.interfaces;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using ClaimTypes = System.Security.Claims.ClaimTypes;
using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.RuntimeEditor;

[Produces("application/json")]
[Route("api/runtime-editor")]
[ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SuperAdminRole)]
public partial class RuntimeEditorController : ControllerBase
{
    private readonly IEditionStorageService runtimeEditorStorageService;

    public RuntimeEditorController(IEditionStorageService runtimeEditorStorageService)
        => this.runtimeEditorStorageService = runtimeEditorStorageService;

    [HttpGet, Route("dictionary")]
    public async Task<IActionResult> GetDictionary(CancellationToken ct)
        => Ok((await runtimeEditorStorageService.GetDictionary(ct))?.Items);

    [HttpGet, Route("edition")]
    public async Task<IActionResult> GetEdition(CancellationToken ct)
        => Ok(await runtimeEditorStorageService.GetEdition(ct));

    [HttpPost, Route("edition")]
    public async Task<IActionResult> SaveEdition([FromBody] RteEdition edition, CancellationToken ct)
    {
        await runtimeEditorStorageService.SaveEdition(edition, User.FindFirstValue(ClaimTypes.NameIdentifier), ct);
        return NoContent();
    }
}
