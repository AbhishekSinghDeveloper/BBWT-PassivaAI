using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-request")]
public class FormIORequestController : DataControllerBase<FormRequest, FormRequestDTO, FormRequestPageDTO>
{
    private readonly IFormIORequestService _formIORequestService;

    public FormIORequestController(IDataService dataService, IFormIORequestService formIORequestService)
        : base(dataService, formIORequestService)
    {
        _formIORequestService = formIORequestService;
    }

    [HttpGet]
    [Route("targets")]
    public async Task<IActionResult> GetTargets(CancellationToken ct = default)
    {
        return Ok(await _formIORequestService.GetTargets(ct));
    }

    [HttpPost]
    [Route("new")]
    public async Task<IActionResult> NewRequest([FromBody] FormRequestDTO request, CancellationToken ct = default)
    {
        return Ok(await _formIORequestService.CreateNewRequest(request, ct));
    }
}
