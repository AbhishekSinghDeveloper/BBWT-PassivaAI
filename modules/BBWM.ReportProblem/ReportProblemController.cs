using BBWM.Core.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.ReportProblem;

[Produces("application/json")]
[Route("api/report-problem")]
public class ReportProblemController : ControllerBase
{
    private readonly IReportProblemService _reportProblemService;

    public ReportProblemController(IReportProblemService reportProblemService)
        => _reportProblemService = reportProblemService;

    [HttpPost]
    public Task<IActionResult> Send([FromBody] ReportProblemDTO reportProblem)
        => NoContent(() => _reportProblemService.Send(
            reportProblem, HttpContext.Request.Headers["User-Agent"], HttpContext.GetDomainUrl()));


    /// <summary>
    /// Sends information about an error to create PTS task if user authenticated.
    /// 
    /// Note! This method is supposed to be triggered for ANY AUTHENTICATED user. You may want to add some
    /// throttling filtration inside the method (or by setting [Authorize(Roles = ...)]) if you don't trust some
    /// category of authenticated users who may send flooding messages.
    /// For example, you trust authenticated users who purchased a subscription, you don't trust authenticated users who
    /// signed up but only use free services of the website.
    /// 
    /// </summary>
    /// <param name="errorLogDto">Error data.</param>
    [HttpPost]
    [Route("auto-send"), ResponseCache(NoStore = true)]
    public async Task<IActionResult> AutoSend([FromBody] ErrorLogDTO errorLogDto)
        => await NoContent(() => _reportProblemService.AutoSend(errorLogDto));
}
