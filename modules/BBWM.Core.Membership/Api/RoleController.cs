using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.Core.Web.ModelBinders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Membership.Api;

[Route("api/role")]
[ReadWriteAuthorize(
    ReadRoles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole,
    WriteRoles = Core.Roles.SuperAdminRole)]
public class RoleController : DataControllerBase<Role, RoleDTO, RoleDTO, string>
{
    private readonly IRoleGitDataService _roleGitDataService;
    private readonly IApiAccessModelGetter _apiAccessModelGetter;
    private readonly IRoleService _roleService;

    public RoleController(
        IDataService dataService,
        IRoleService roleService,
        IRoleGitDataService roleGitDataService,
        IApiAccessModelGetter apiSecurityModelGetter)
        : base(dataService, roleService)
    {
        _roleService = roleService;
        _roleGitDataService = roleGitDataService;
        _apiAccessModelGetter = apiSecurityModelGetter;
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromBody] RoleDTO dto,
        [FromServices] IModelHashingService modelHashingService, CancellationToken ct = default)
    {
        var result = await _roleService.Create(dto, ct);
        await OnRolesChanged(ct);
        return Request.CreatedResult<RoleDTO, string>(result, modelHashingService);
    }

    [HttpPut, Route("{id}")]
    public override async Task<IActionResult> Update([FromBody] RoleDTO dto, CancellationToken ct = default)
    {
        var result = await _roleService.Update(dto, ct);
        await OnRolesChanged(ct);
        return Ok(result);
    }

    [HttpDelete, Route("{id}")]
    public override async Task<IActionResult> Delete([HashedKeyBinder] string id, CancellationToken ct = default)
    {
        await _roleService.Delete(id, ct);
        await OnRolesChanged(ct);
        return Ok(id);
    }

    [HttpGet]
    [Route("core")]
    public IActionResult GetCoreRoles() => Ok(_roleService.GetHardcodedRoles());

    [HttpGet]
    [Route("project")]
    public IActionResult GetProjectRoles() => Ok(_roleService.GetProjectRoles());

    [HttpGet]
    [Route("model")]
    [AllowAnonymous]
    public IActionResult GetApiAccessModel()
        => Ok(_apiAccessModelGetter.GetApiAccessModel());

    private Task OnRolesChanged(CancellationToken ct)
        => _roleGitDataService.SendToGit(ct);

}
