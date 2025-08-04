using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Chart.DTO;
using BBF.Reporting.Widget.Chart.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.Chart.Api;

[Route("api/reporting3/widget/chart")]
[Authorize]
public class WidgetChartController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetChartViewService _chartViewService;
    private readonly IWidgetChartDataService _chartDataService;
    private readonly IWidgetChartBuilderService _chartBuilderService;

    public WidgetChartController(
        IWidgetChartViewService chartViewService,
        IWidgetChartDataService chartDataService,
        IWidgetChartBuilderService chartBuilderService)
    {
        _chartViewService = chartViewService;
        _chartDataService = chartDataService;
        _chartBuilderService = chartBuilderService;
    }

    [HttpGet("{widgetSourceId}/view")]
    public async Task<IActionResult> GetView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _chartViewService.GetView(widgetSourceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ChartBuildDTO chart, CancellationToken ct)
        => Ok(await _chartBuilderService.Create(chart, ct));

    [HttpPost("create-draft/{widgetSourceReleaseId?}")]
    public async Task<IActionResult> CreateDraft(Guid? widgetSourceReleaseId,
        [FromBody] ChartBuildDTO chart, CancellationToken ct)
        => Ok(await _chartBuilderService.CreateDraft(chart, widgetSourceReleaseId, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] ChartBuildDTO chart, CancellationToken ct)
        => Ok(await _chartBuilderService.Update(chart, ct));

    [HttpGet("{querySourceId}/query-schema")]
    public async Task<IActionResult> GetQuerySchema(Guid querySourceId, CancellationToken ct)
        => Ok(await _chartDataService.GetQuerySchema(querySourceId, ct));

    [HttpGet("{querySourceId}/query-data")]
    public async Task<IActionResult> GetQueryDataRows(Guid querySourceId,
        [FromQuery] QueryVariables queryVariables, CancellationToken ct)
        => Ok(await _chartDataService.GetQueryDataRows(querySourceId, queryVariables, ct));

    [HttpGet("{querySourceId}/view-metadata")]
    public async Task<IActionResult> GetViewMetadata(Guid querySourceId, CancellationToken ct)
        => Ok(await _chartDataService.GetViewMetadata(querySourceId, ct));

    [HttpPut("release-draft/{widgetSourceDraftId}")]
    public async Task<IActionResult> ReleaseDraft(Guid widgetSourceDraftId, CancellationToken ct)
        => Ok(await _chartBuilderService.ReleaseDraft(widgetSourceDraftId, ct));
}