using Microsoft.AspNetCore.Mvc;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio")]
public class FormIODefinitionController : DataControllerBase<FormDefinition, FormDefinitionDTO, FormDefinitionPageDTO>
{
    private readonly IFormIODefinitionService _formIODefinitionService;

    public FormIODefinitionController(IDataService dataService, IFormIODefinitionService formIODefinitionService)
        : base(dataService, formIODefinitionService)
    {
        _formIODefinitionService = formIODefinitionService;
    }

    [HttpPost]
    [Route("form-definition/{id}/{formRevisionId}/{readOnly}")]
    public async Task<IActionResult> GetFormDefinition([HashedKeyBinder] int id,
        [HashedKeyBinder(typeof(FormRevisionDTO), "Id")]
        int formRevisionId, bool readOnly,
        [FromBody] FormDefinitionParameters formDefinitionParameters, CancellationToken ct = default)
    {
        return Ok(await _formIODefinitionService.GetFormDefinitionJson(id, formRevisionId, readOnly, formDefinitionParameters.ParameterString, ct));
    }

    [HttpPost]
    [Route("form-definition")]
    public async Task<IActionResult> AddFormDefinition([FromBody] FormDefinitionForNewRequestDTO formDefinitionForNewRequest, CancellationToken ct = default)
    {
        return Ok(await _formIODefinitionService.Create(formDefinitionForNewRequest, ct));
    }

    [HttpPost]
    [Route("copy-form/{id}")]
    public async Task<IActionResult> CopyFormDefinition(int id, CancellationToken ct = default)
    {
        var originalForm = await _formIODefinitionService.Get(id, ct);
        return Ok(await _formIODefinitionService.Copy(originalForm, ct));
    }

    [HttpPost]
    [Route("publish")]
    public async Task<IActionResult> PublishFormDefinition([FromBody] PublishFormDefinitionDTO publishFormDefinition,
        CancellationToken ct = default)
    {
        return Ok(await _formIODefinitionService.PublishFormDefinition(publishFormDefinition, ct));
    }

    [HttpPost]
    [Route("owner")]
    public async Task<IActionResult> ChangeOwnerFormDefinition([FromBody] ChangeFormDefinitionOwnerDTO changeFormDefinitionOwner,
        CancellationToken ct = default)
    {
        return Ok(await _formIODefinitionService.ChangeFormDesignOwnership(changeFormDefinitionOwner, ct));
    }

    [HttpGet]
    [Route("available-versions")]
    public IActionResult GetAvailableVersionsForFiltering(
        [FromQuery] List<int> orgIds,
        [FromQuery] bool isAdmin,
        [FromQuery] string userId,
        CancellationToken ct = default)
    {
        return Ok(_formIODefinitionService.GetAvailableVersionsForFiltering(orgIds, isAdmin, userId, ct));
    }
}