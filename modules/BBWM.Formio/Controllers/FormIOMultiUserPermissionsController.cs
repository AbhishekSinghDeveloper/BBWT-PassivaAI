using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/multi-user-form-permissions")]
public class FormIOMultiUserPermissionsController
    : DataControllerBase<MultiUserFormStagePermissions, MultiUserFormStagePermissionsDTO, MultiUserFormStagePermissionsDTO>
{
    private readonly IFormIOMultiUserFormPermissionsService _formIOMultiUserFormPermissionsService;

    public FormIOMultiUserPermissionsController(IDataService dataService, IFormIOMultiUserFormPermissionsService formIOMultiUserFormPermissionsService)
        : base(dataService, formIOMultiUserFormPermissionsService)
    {
        _formIOMultiUserFormPermissionsService = formIOMultiUserFormPermissionsService;
    }

    [HttpPost]
    [Route("new-permission")]
    public async Task<IActionResult> NewMultiUserForm([FromBody] NewMultiUserFormPermissionDTO dto, CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormPermissionsService.NewMultiUserStagePermission(dto, ct));
    }
}