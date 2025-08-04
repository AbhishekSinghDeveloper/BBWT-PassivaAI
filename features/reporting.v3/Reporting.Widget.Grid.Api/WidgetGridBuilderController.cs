using BBF.Reporting.Widget.Grid.DTO;
using BBF.Reporting.Widget.Grid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.Grid.Api;

[Route("api/reporting3/widget/grid/builder")]
[Authorize]
public class WidgetGridBuilderController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetGridBuilderService _widgetBuilderService;

    public WidgetGridBuilderController(IWidgetGridBuilderService widgetBuilderService)
        => _widgetBuilderService = widgetBuilderService;

    [HttpGet("{widgetSourceId}/view")]
    public async Task<IActionResult> GetView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _widgetBuilderService.GetView(widgetSourceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GridViewDTO dto, CancellationToken ct)
        => Ok(await _widgetBuilderService.Create(dto, ct));

    [HttpPost("create-draft/{widgetSourceReleaseId?}")]
    public async Task<IActionResult> CreateDraft(Guid? widgetSourceReleaseId,
        [FromBody] GridViewDTO grid, CancellationToken ct)
        => Ok(await _widgetBuilderService.CreateDraft(grid, widgetSourceReleaseId, ct));

    [HttpPut("{widgetSourceId}")]
    public async Task<IActionResult> Update([FromBody] GridViewDTO dto, CancellationToken ct)
        => Ok(await _widgetBuilderService.Update(dto, ct));

    [HttpPut("release-draft/{widgetSourceDraftId}")]
    public async Task<IActionResult> ReleaseDraft(Guid widgetSourceDraftId, CancellationToken ct)
        => Ok(await _widgetBuilderService.ReleaseDraft(widgetSourceDraftId, ct));
}