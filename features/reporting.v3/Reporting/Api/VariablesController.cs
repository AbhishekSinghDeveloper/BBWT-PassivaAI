using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Api;

[Route("api/reporting3/variables")]
[Authorize]
public class VariablesController : DataControllerBase<Variable, VariableDTO, VariableDTO>
{
    private readonly IVariablesService _variablesService;

    public VariablesController(IDataService dataService, IVariablesService variablesService) :
        base(dataService)
        => _variablesService = variablesService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await _variablesService.GetAll(ct));
}