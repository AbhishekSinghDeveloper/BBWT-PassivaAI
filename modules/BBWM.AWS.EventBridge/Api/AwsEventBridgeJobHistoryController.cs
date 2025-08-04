using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Filters;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

namespace BBWM.AWS.EventBridge.Api;

[Produces(MediaTypeNames.Application.Json)]
[Route("api/aws-event-bridge-history")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AwsEventBridgeJobHistoryContoller : Core.Web.ControllerBase
{
    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage(
        [FromQuery] QueryCommand command,
        [FromServices] IDataService dataService,
        CancellationToken ct = default)
        => Ok(
            await dataService.GetPage<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(
                command, sorter: (q, s) => q.OrderByDescending(h => h.StartTime), ct: ct));

    [HttpGet("canceled-jobs-page")]
    public async Task<IActionResult> GetCanceledJobsPageAsync(
        [FromQuery] QueryCommand command,
        [FromServices] IDataService dataService,
        CancellationToken ct = default)
       => Ok(
           await dataService.GetPage<EventBridgeJobHistory, AwsEventBridgeJobHistoryDTO>(
               command,
               filter: qf => qf.SetQuery(
                                qf.Query.Where(
                                    h => h.CompletionStatus == JobCompletionStatus.CanceledByShutdown ||
                                         h.CompletionStatus == JobCompletionStatus.CanceledByUser)),
               sorter: (q, s) => q.OrderByDescending(h => h.StartTime),
               ct: ct));
}
