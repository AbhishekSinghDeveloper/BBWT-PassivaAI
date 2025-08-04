using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-data-draft")]
public class FormIODataDraftController : DataControllerBase<FormData, FormDataDTO, FormDataPageDTO>
{
    private readonly IFormIODataService _formIODataService;

    public FormIODataDraftController(IDataService dataService, IFormIODataService formIODataService)
        : base(dataService, formIODataService)
    {
        _formIODataService = formIODataService;
    }

    [HttpPost]
    [Route("draft-data/{formDefinitionId}/{userId}")]
    public async Task<IActionResult> GetFormDataDraft(
        [HashedKeyBinder(typeof(FormDefinitionDTO), "Id")]
        int formDefinitionId, string userId, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.GetFormDataDraft(formDefinitionId, userId, ct));
    }

    [HttpGet]
    [Route("remove-draft/{draftId}")]
    public async Task<IActionResult> RemoveDataDraft(int draftId, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.DiscardDraft(draftId, true, ct));
    }

    [HttpPost]
    [Route("draft-data")]
    public async Task<IActionResult> AddFormData([FromBody] FormDataDraftDTO formData, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.SaveFormDataDraft(formData, ct));
    }
}