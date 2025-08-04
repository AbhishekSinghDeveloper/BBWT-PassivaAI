using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-revision")]
public class FormIORevisionController : DataControllerBase<FormRevision, FormRevisionDTO, FormRevisionDTO>
{
    private readonly IFormioIORevisionService _formioIORevisionService;

    public FormIORevisionController(IDataService dataService, IFormioIORevisionService formioIORevisionService) : base(dataService, formioIORevisionService)
    {
        _formioIORevisionService = formioIORevisionService;
    }

    [HttpPost]
    [Route("new")]
    public async Task<IActionResult> Create([FromBody] NewFormRevisionRequestDTO dto, CancellationToken ct = default)
    {
        return Ok(await _formioIORevisionService.Create(dto, ct));
    }

    [HttpPost]
    [Route("{formRevisionId}")]
    public async Task<IActionResult> Update(
        [HashedKeyBinder(typeof(FormRevisionDTO), "Id")]
        int formRevisionId,
        [FromBody] UpdateFormRevisionRequestDTO dto,
        CancellationToken ct = default)
    {
        return Ok(await _formioIORevisionService.Update(formRevisionId, dto, ct));
    }

    [HttpPost]
    [Route("active/{formDefinitionId}/{revisionId}")]
    public async Task<IActionResult> SetActive(
        [HashedKeyBinder(typeof(FormDefinitionDTO), "Id")]
        int formDefinitionId,
        [HashedKeyBinder(typeof(FormRevisionDTO), "Id")]
        int revisionId, CancellationToken ct = default)
    {
        return Ok(await _formioIORevisionService.SetActive(formDefinitionId, revisionId, ct));
    }
}