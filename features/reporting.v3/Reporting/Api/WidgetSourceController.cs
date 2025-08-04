using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Web;
using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Api;

[Route("api/reporting3/widget-source")]
[Authorize]
[NamedWidgetSourceAuthorize]
public class WidgetSourceController : DataControllerBase<WidgetSource, WidgetSourceDTO, WidgetSourceDTO, Guid>
{
    private readonly INamedWidgetSourceService _namedWidgetSourceService;

    public WidgetSourceController(
        IDataService dataService,
        IWidgetSourceService widgetSourceService,
        INamedWidgetSourceService namedWidgetSourceService)
        : base(dataService, widgetSourceService)
        => _namedWidgetSourceService = namedWidgetSourceService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await _namedWidgetSourceService.GetAll(ct));

    [HttpGet, Route("page")]
    public override async Task<IActionResult> GetPage([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        // Agreed max no of returned records.
        if (command.Take is null or > 100) command.Take = 100;
        return Ok(await _namedWidgetSourceService.GetPage(command, ct));
    }

    /// <summary>
    /// Gets widget preload details for further loading of specific widget type by container widget component.
    /// </summary>
    [HttpGet("{widgetCode}/preload")]
    public async Task<IActionResult> GetWidgetSourcePreload(string widgetCode, CancellationToken ct = default)
        => Ok(await _namedWidgetSourceService.GetByCode(widgetCode, ct));

    [HttpPut, Route("{widgetSourceId}/publish")]
    public async Task<IActionResult> PublishWidget(Guid widgetSourceId, [FromBody] IEnumerable<int> organizationIds,
        CancellationToken ct = default)
    {
        await _namedWidgetSourceService.PublishWidget(widgetSourceId, organizationIds, ct);
        return Ok();
    }

    [HttpPut, Route("{widgetSourceId}/change-owner")]
    public async Task<IActionResult> ChangeOwner(Guid widgetSourceId, [FromQuery] string ownerId,
        CancellationToken ct = default)
    {
        await _namedWidgetSourceService.ChangeOwner(widgetSourceId, ownerId, ct);
        return Ok();
    }
}