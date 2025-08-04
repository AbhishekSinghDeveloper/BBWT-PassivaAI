using BBF.Reporting.Widget.Grid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.Grid.Api;

[Route("api/reporting3/widget/grid/view")]
[Authorize]
public class WidgetGridViewController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetGridViewService _widgetViewService;

    public WidgetGridViewController(IWidgetGridViewService widgetViewService) => _widgetViewService = widgetViewService;

    [HttpGet("{widgetSourceId}")]
    public async Task<IActionResult> GetDisplayView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _widgetViewService.GetView(widgetSourceId, ct));
}