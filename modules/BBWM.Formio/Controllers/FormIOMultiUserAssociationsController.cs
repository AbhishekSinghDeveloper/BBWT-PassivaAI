using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/multi-user-form-associations")]
public class FormIOMultiUserAssociationsController : DataControllerBase<MultiUserFormAssociations, MultiUserFormAssociationsDTO, MultiUserFormAssociationsDTO>
{
    private readonly IFormIOMultiUserFormAssociationsService _formIoMultiUserFormAssociationsService;

    public FormIOMultiUserAssociationsController(IDataService dataService, IFormIOMultiUserFormAssociationsService formIOMultiUserFormAssociationsService)
        : base(dataService, formIOMultiUserFormAssociationsService)
    {
        _formIoMultiUserFormAssociationsService = formIOMultiUserFormAssociationsService;
    }

    [HttpPost]
    [Route("new-mufassoc")]
    public async Task<IActionResult> NewMultiUserForm([FromBody] NewMultiUserFormAssociationsDTO dto, CancellationToken ct = default)
    {
        return Ok(await _formIoMultiUserFormAssociationsService.NewMultiUserFormAssociation(dto, ct));
    }

    [HttpGet]
    [Route("render/{id}/{target}")]
    public async Task<IActionResult> GetMUFDataForRendering(
        [HashedKeyBinder(typeof(MultiUserFormAssociationsDTO), "Id")]
        int id, string target, CancellationToken ct = default)
    {
        return Ok(await _formIoMultiUserFormAssociationsService.GetMUFDataForRendering(id, target, ct));
    }
}