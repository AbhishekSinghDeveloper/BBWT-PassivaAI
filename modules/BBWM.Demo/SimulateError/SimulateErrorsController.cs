using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Demo.SimulateError;

[Produces("application/json")]
[Route("api/demo/simulate-error")]
public class SimulateErrorsController : ControllerBase
{
    private readonly ISimulateErrorService _simulateErrorService;


    public SimulateErrorsController(ISimulateErrorService simulateErrorService)
        => _simulateErrorService = simulateErrorService;

    [HttpPost]
    public IActionResult SimulateErrors([FromBody] SimulateErrorCodeDTO dto)
    {
        switch (dto.Code)
        {
            case StatusCodes.Status403Forbidden:
                return Forbid();
            case StatusCodes.Status404NotFound:
                return NotFound();
            case SQLStatusMessages.Msg801Level16State1:
                _simulateErrorService.SimulateSQLError();
                break;
            default: return StatusCode(dto.Code);
        }

        return StatusCode(dto.Code);
    }


    [HttpPost, Route("bad-request")]
    public IActionResult BadRequest([FromBody] SimulateBadRequestDTO dto) =>
        BadRequest(new Dictionary<string, string>
        {
            ["First Field"] = dto.FirstField,
            ["Second Field"] = dto.SecondField
        });

    [HttpGet]
    [Route("exception")]
    public IActionResult SimulateException() =>
        throw new Exception("Exception simulation.");
}
