using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/multi-user-form")]
public class FormIOMultiUserDefinitionController : DataControllerBase<MultiUserFormDefinition, MultiUserFormDefinitionDTO, MultiUserFormDefinitionDTO>
{
    private readonly IFormIOMultiUserFormDefinitionService _formIOMultiUserFormDefinitionService;

    public FormIOMultiUserDefinitionController(IDataService dataService, IFormIOMultiUserFormDefinitionService formIOMultiUserFormDefinitionService)
        : base(dataService, formIOMultiUserFormDefinitionService)
    {
        _formIOMultiUserFormDefinitionService = formIOMultiUserFormDefinitionService;
    }

    [HttpGet]
    [Route("form-definitions")]
    public async Task<IActionResult> GetFormDefinitions(CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormDefinitionService.GetFormDefinitions(ct));
    }

    [HttpPost]
    [Route("new-muf")]
    public async Task<IActionResult> NewMultiUserForm([FromBody] NewMultiUserFormDefinitionDTO dto, CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormDefinitionService.NewMultiUserForm(dto, ct));
    }

    [HttpGet]
    [Route("user-targets")]
    public async Task<IActionResult> GetPossibleTargets(CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormDefinitionService.GetPossibleTargets(ct));
    }

    [HttpGet]
    [Route("instance-targets/{id}")]
    public async Task<IActionResult> GetInstanceTargets(
        [HashedKeyBinder(typeof(MultiUserFormDefinitionDTO), "Id")]
        int id, CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormDefinitionService.GetInstanceTargets(id, ct));
    }

    [HttpGet]
    [Route("muf-ready/{id}")]
    public async Task<IActionResult> IsMUFReady(
        [HashedKeyBinder(typeof(MultiUserFormDefinitionDTO), "Id")]
        int id, CancellationToken ct = default)
    {
        return Ok(await _formIOMultiUserFormDefinitionService.IsMUFReady(id, ct));
    }
}