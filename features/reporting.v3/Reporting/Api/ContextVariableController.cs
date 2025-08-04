using BBF.Reporting.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Api;

[Route("api/reporting3/query/context-variables")]
[Authorize]
public class ContextVariableController : BBWM.Core.Web.ControllerBase
{
    private readonly IContextVariableService _contextVariableService;

    public ContextVariableController(IContextVariableService contextVariableService)
        => _contextVariableService = contextVariableService;

    [HttpGet]
    public IActionResult GetContextVariableNames() => Ok(_contextVariableService.GetVariableNames());
}
