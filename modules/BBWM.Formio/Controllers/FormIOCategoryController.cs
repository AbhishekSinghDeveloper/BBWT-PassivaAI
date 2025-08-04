using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-category")]
public class FormIOCategoryController : DataControllerBase<FormCategory, FormCategoryDTO, FormCategoryDTO>
{
    private readonly IFormIOCategoryService _formIOCategoryService;

    public FormIOCategoryController(IDataService dataService, IFormIOCategoryService formIOCategoryService)
        : base(dataService, formIOCategoryService)
    {
        _formIOCategoryService = formIOCategoryService;
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        return Ok(await _formIOCategoryService.GetAll(ct));
    }
}
