using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Grid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.Grid.Api;

[Route("api/reporting3/widget/grid/data")]
[Authorize]
public class WidgetGridDataController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetGridDataService _widgetDataService;

    public WidgetGridDataController(IWidgetGridDataService widgetDataService) => _widgetDataService = widgetDataService;

    [HttpGet("{querySourceId}/query-data")]
    public async Task<IActionResult> GetQueryDataRows(Guid querySourceId,
        [FromQuery] QueryPageRequest pageRequest, CancellationToken ct)
        => Ok(await _widgetDataService.GetQueryDataRows(querySourceId, pageRequest, ct));

    [HttpGet("{querySourceId}/query-data-count")]
    public async Task<IActionResult> GetQueryDataRowsCount(Guid querySourceId,
        [FromQuery] QueryVariables queryVariables, CancellationToken ct)
        => Ok(await _widgetDataService.GetQueryDataRowsCount(querySourceId, queryVariables, ct));

    [HttpGet("{querySourceId}/query-data-aggregations")]
    public async Task<IActionResult> GetQueryDataAggregations(Guid querySourceId,
        [FromQuery] IList<QueryColumnAggregation> aggregations,
        [FromQuery] QueryVariables queryVariables, CancellationToken ct)
        => Ok(await _widgetDataService.GetQueryDataAggregations(querySourceId, aggregations, queryVariables, ct));

    [HttpGet("{querySourceId}/query-schema")]
    public async Task<IActionResult> GetQuerySchema(Guid querySourceId, CancellationToken ct)
        => Ok(await _widgetDataService.GetQuerySchema(querySourceId, ct));

    [HttpGet("{querySourceId}/view-metadata")]
    public async Task<IActionResult> GetViewMetadata(Guid querySourceId, CancellationToken ct)
        => Ok(await _widgetDataService.GetViewMetadata(querySourceId, ct));
}