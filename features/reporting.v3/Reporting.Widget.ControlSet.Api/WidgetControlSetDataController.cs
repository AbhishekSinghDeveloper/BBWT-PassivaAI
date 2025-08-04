using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Widget.ControlSet.Api;

[Route("api/reporting3/widget/control-set/data")]
[Authorize]
public class WidgetControlSetDataController : BBWM.Core.Web.ControllerBase
{
    private readonly IWidgetControlSetDataService _widgetDataService;

    public WidgetControlSetDataController(IWidgetControlSetDataService widgetDataService)
        => _widgetDataService = widgetDataService;

    [HttpGet("dropdown-data")]
    public async Task<IActionResult> GetDropdownData([FromQuery] QueryDataRequest request, CancellationToken ct)
        => Ok(await _widgetDataService.GetDropdownData(request, ct));
}