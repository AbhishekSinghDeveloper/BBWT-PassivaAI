using BBF.Reporting.Dashboard.DTO;
using BBF.Reporting.Dashboard.Interfaces;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Dashboard.Services;

public class DashboardViewService : IDashboardViewService
{
    private readonly IDataService _dataService;

    public DashboardViewService(IDataService dataService)
        => _dataService = dataService;

    public async Task<DashboardViewDTO> GetView(Guid dashboardId, CancellationToken ct)
        => await _dataService.Get<DbModel.Dashboard, DashboardViewDTO, Guid>(dashboardId, query =>
            query.Include(dashboard => dashboard.Widgets)
                .ThenInclude(widget => widget.WidgetSource), ct);

    public async Task<DashboardViewDTO> GetViewByCode(string dashboardCode, CancellationToken ct)
        => await _dataService.Get<DbModel.Dashboard, DashboardViewDTO>(query =>
            query.Include(dashboard => dashboard.Widgets)
                .ThenInclude(widget => widget.WidgetSource)
                .Where(dashboard => dashboard.UrlSlug == dashboardCode), ct);
}