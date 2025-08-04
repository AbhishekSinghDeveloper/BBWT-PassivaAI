using BBF.Reporting.Widget.Html.DTO;
using BBF.Reporting.Widget.Html.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.Html.Api;

[Route("api/reporting3/widget/html")]
[Authorize]
public class WidgetHtmlController : BBWM.Core.Web.ControllerBase
{
    // ReSharper disable once NotAccessedField.Local
    private readonly IWidgetHtmlViewService _htmlViewService;
    private readonly IWidgetHtmlBuilderService _htmlBuilderService;

    public WidgetHtmlController(
        IWidgetHtmlViewService htmlViewService,
        IWidgetHtmlBuilderService htmlBuilderService)
    {
        _htmlViewService = htmlViewService;
        _htmlBuilderService = htmlBuilderService;
    }

    [HttpGet("{widgetSourceId}/view")]
    public async Task<IActionResult> GetView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _htmlViewService.GetView(widgetSourceId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HtmlDTO html, CancellationToken ct)
        => Ok(await _htmlBuilderService.Create(html, ct));

    [HttpPost("create-draft/{widgetSourceReleaseId?}")]
    public async Task<IActionResult> CreateDraft(Guid? widgetSourceReleaseId,
        [FromBody] HtmlDTO html, CancellationToken ct)
        => Ok(await _htmlBuilderService.CreateDraft(html, widgetSourceReleaseId, ct));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] HtmlDTO html, CancellationToken ct)
        => Ok(await _htmlBuilderService.Update(html, ct));

    [HttpPut("release-draft/{widgetSourceDraftId}")]
    public async Task<IActionResult> ReleaseDraft(Guid widgetSourceDraftId, CancellationToken ct)
        => Ok(await _htmlBuilderService.ReleaseDraft(widgetSourceDraftId, ct));
}