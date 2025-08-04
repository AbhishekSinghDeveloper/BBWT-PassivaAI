using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/formio-parameters")]
public class FormIOParameterListController : DataControllerBase<FormParameterList, FormParameterListDTO, FormParameterListDTO>
{
    public FormIOParameterListController(IDataService dataService, IFormIOParameterListService formIOParameterListService)
        : base(dataService, formIOParameterListService)
    {
    }
}