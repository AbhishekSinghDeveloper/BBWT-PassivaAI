using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/multi-user-form-stage")]
public class FormIOMultiUserStageController : DataControllerBase<MultiUserFormStage, MultiUserFormStageDTO, MultiUserFormStageDTO>
{
    private readonly IFormIOMultiUserFormStageService _formIoMultiUserFormStageService;

    public FormIOMultiUserStageController(IDataService dataService, IFormIOMultiUserFormStageService formIOMultiUserFormStageService)
        : base(dataService, formIOMultiUserFormStageService)
    {
        _formIoMultiUserFormStageService = formIOMultiUserFormStageService;
    }

    [HttpGet]
    [Route("group-targets")]
    public async Task<IActionResult> GetPossibleTargets(CancellationToken ct = default)
    {
        return Ok(await _formIoMultiUserFormStageService.GetPossibleTargets(ct));
    }

    [HttpPost]
    [Route("update-stage")]
    public async Task<IActionResult> NewMultiUserForm([FromBody] MultiUserFormStageUpdateDTO dto, CancellationToken ct = default)
    {
        return Ok(await _formIoMultiUserFormStageService.UpdateMultiUserStage(dto, ct));
    }
}