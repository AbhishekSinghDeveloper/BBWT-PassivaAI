using BBF.Reporting.Dashboard.DTO;
using BBF.Reporting.Dashboard.Interfaces;
using BBWM.Core.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Dashboard.Api;

[Route("api/reporting3/dashboard")]
[DashboardAuthorize]
public class DashboardController : BBWM.Core.Web.ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IDashboardViewService _dashboardViewService;
    private readonly IDashboardBuilderService _dashboardBuilderService;

    public DashboardController(
        IDashboardService dashboardService,
        IDashboardBuilderService dashboardBuilderService,
        IDashboardViewService dashboardViewService)
    {
        _dashboardService = dashboardService;
        _dashboardViewService = dashboardViewService;
        _dashboardBuilderService = dashboardBuilderService;
    }

    [HttpGet("{dashboardId}/build")]
    public async Task<IActionResult> GetBuild(Guid dashboardId, CancellationToken ct = default)
        => Ok(await _dashboardBuilderService.GetBuild(dashboardId, ct));

    [HttpGet("{dashboardId}/view")]
    public async Task<IActionResult> GetView(Guid dashboardId, CancellationToken ct = default)
        => Ok(await _dashboardViewService.GetView(dashboardId, ct));

    [HttpGet("{dashboardCode}/view-by-code")]
    public async Task<IActionResult> GetViewByUrlSlug(string dashboardCode, CancellationToken ct = default)
        => Ok(await _dashboardViewService.GetViewByCode(dashboardCode, ct));

    [HttpGet, Route("page")]
    public virtual async Task<IActionResult> GetPage([FromQuery] QueryCommand command, CancellationToken ct = default)
        => Ok(await _dashboardService.GetPage(command, ct));

    public async Task<IActionResult> Create([FromBody] DashboardBuildDTO dto, CancellationToken ct = default)
        => Ok(await _dashboardBuilderService.Create(dto, ct));

    [HttpPut, Route("{dashboardId}")]
    public async Task<IActionResult> Update([FromBody] DashboardBuildDTO dashboard, CancellationToken ct = default)
        => Ok(await _dashboardBuilderService.Update(dashboard, ct));

    [HttpDelete, Route("{dashboardId}")]
    public async Task<IActionResult> Delete(Guid dashboardId, CancellationToken ct = default)
    {
        await _dashboardService.Delete(dashboardId, ct);
        return Ok();
    }

    [HttpPut, Route("{dashboardId}/publish")]
    public async Task<IActionResult> PublishDashboard(Guid dashboardId, [FromBody] IEnumerable<int> organizationIds,
        CancellationToken ct = default)
    {
        await _dashboardService.PublishDashboard(dashboardId, organizationIds, ct);
        return Ok();
    }

    [HttpPut, Route("{dashboardId}/change-owner")]
    public async Task<IActionResult> ChangeOwner(Guid dashboardId, [FromQuery] string ownerId,
        CancellationToken ct = default)
    {
        await _dashboardService.ChangeOwner(dashboardId, ownerId, ct);
        return Ok();
    }
}