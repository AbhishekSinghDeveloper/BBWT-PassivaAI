using BBF.Reporting.Widget.ControlSet.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.ControlSet.Api;

[Route("api/reporting3/widget/control-set/view")]
[Authorize]
public class WidgetControlSetViewController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetControlSetViewService _widgetViewService;

    public WidgetControlSetViewController(IWidgetControlSetViewService widgetViewService)
        => _widgetViewService = widgetViewService;

    [HttpGet("{widgetSourceId}")]
    public async Task<IActionResult> GetDisplayView(Guid widgetSourceId, CancellationToken ct)
        => Ok(await _widgetViewService.GetView(widgetSourceId, ct));
}