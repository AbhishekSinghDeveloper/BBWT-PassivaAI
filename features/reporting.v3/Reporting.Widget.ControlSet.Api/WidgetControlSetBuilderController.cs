using BBF.Reporting.Widget.ControlSet.DTO;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.ControlSet.Api;

[Route("api/reporting3/widget/control-set/builder")]
[Authorize]
public class WidgetControlSetBuilderController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetControlSetBuilderService _widgetBuilderService;

    public WidgetControlSetBuilderController(IWidgetControlSetBuilderService widgetBuilderService)
        => _widgetBuilderService = widgetBuilderService;

    [HttpGet("{widgetSourceId}/view")]
    public async Task<IActionResult> GetView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _widgetBuilderService.GetView(widgetSourceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ControlSetViewDTO dto, CancellationToken ct)
        => Ok(await _widgetBuilderService.Create(dto, ct));

    [HttpPost("create-draft/{widgetSourceReleaseId?}")]
    public async Task<IActionResult> CreateDraft(Guid? widgetSourceReleaseId,
        [FromBody] ControlSetViewDTO controlSet, CancellationToken ct)
        => Ok(await _widgetBuilderService.CreateDraft(controlSet, widgetSourceReleaseId, ct));

    [HttpPut("{widgetSourceId}")]
    public async Task<IActionResult> Update([FromBody] ControlSetViewDTO dto, CancellationToken ct)
        => Ok(await _widgetBuilderService.Update(dto, ct));

    [HttpPut("release-draft/{widgetSourceDraftId}")]
    public async Task<IActionResult> ReleaseDraft(Guid widgetSourceDraftId, CancellationToken ct)
        => Ok(await _widgetBuilderService.ReleaseDraft(widgetSourceDraftId, ct));
}