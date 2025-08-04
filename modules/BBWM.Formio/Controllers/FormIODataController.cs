using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.Classes;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Interfaces.FormVersioningInterfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-data")]
public class FormIODataController : DataControllerBase<FormData, FormDataDTO, FormDataPageDTO>
{
    private readonly IFormIODataService _formIODataService;
    private readonly IFormDataVersioningService _formDataVersioningService;

    public FormIODataController(
        IDataService dataService,
        IFormIODataService formIODataService,
        IFormDataVersioningService formDataVersioningService)
        : base(dataService, formIODataService)
    {
        _formIODataService = formIODataService;
        _formDataVersioningService = formDataVersioningService;
    }

    [HttpPost]
    [Route("form-data/{id}")]
    public async Task<IActionResult> GetFormData([HashedKeyBinder] int id, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.GetFormDataJson(id, ct));
    }

    [HttpPost]
    [Route("form-data")]
    public async Task<IActionResult> AddFormData([FromBody] FormDataDTO formData, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.SaveFormData(formData, ct));
    }

    [HttpPost]
    [Route("update-form-data/{formDefinitionId}")]
    public Task<IActionResult> UpdateFormData(
        [HashedKeyBinder(typeof(FormDefinitionDTO), "Id")]
        int formDefinitionId, [FromBody] IEnumerable<FormFieldDataUpdate> updates, CancellationToken ct = default)
    {
        _formDataVersioningService.UpdateFormDataInBackground(formDefinitionId, updates);
        return Task.FromResult<IActionResult>(Ok());
    }

    [HttpGet]
    [Route("has-data/{formDefinitionId}")]
    public async Task<IActionResult> CheckIfFormHasData(
        [HashedKeyBinder(typeof(FormDefinitionDTO), "Id")]
        int formDefinitionId, CancellationToken ct = default)
    {
        return Ok(await _formIODataService.FormHasData(formDefinitionId, ct));
    }

    [HttpGet]
    [Route("available-versions")]
    public IActionResult GetVersionsForFiltering(
        [FromQuery] List<int> orgIds,
        [FromQuery] bool isAdmin,
        CancellationToken ct = default)
    {
        return Ok(_formIODataService.GetVersionsForFiltering(orgIds, isAdmin, ct));
    }

    [HttpDelete]
    [Route("multiple")]
    public async Task<IActionResult> DeleteMultiple([FromQuery] List<int> idsToDelete, CancellationToken ct = default)
    {
        await _formIODataService.DeleteMultiple(idsToDelete, ct);
        return NoContent();
    }
}