using BBF.Reporting.Dashboard.DbModel;
using BBF.Reporting.Dashboard.DTO;
using BBF.Reporting.Dashboard.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;
using BBF.Reporting.Core.Interfaces;

namespace BBF.Reporting.Dashboard.Services;

public class DashboardBuilderService : IDashboardBuilderService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IWidgetProviderFactory _widgetProviderFactory;

    public DashboardBuilderService(
        IDataService dataService,
        ILoggedUserService loggedUserService,
        IWidgetProviderFactory widgetProviderFactory)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
        _widgetProviderFactory = widgetProviderFactory;
    }

    public async Task<DashboardBuildDTO> GetBuild(Guid dashboardId, CancellationToken ct)
        => await _dataService.Get<DbModel.Dashboard, DashboardBuildDTO, Guid>(dashboardId, query =>
            query.Include(dashboard => dashboard.Widgets).ThenInclude(widget => widget.WidgetSource), ct);

    public async Task<DashboardBuildDTO> Create(DashboardBuildDTO build, CancellationToken ct)
    {
        var userId = _loggedUserService.GetLoggedUserId();
        if (userId == null) throw new BusinessException("Cannot get user id to set dashboard owner.");

        return await Create(build, userId, ct);
    }

    public async Task<DashboardBuildDTO> Create(DashboardBuildDTO build, string userId, CancellationToken ct = default)
    {
        await ValidateDashboard(build, ct);

        return await _dataService.Create<DbModel.Dashboard, DashboardBuildDTO>(build, (dashboard, _) =>
        {
            dashboard.OwnerId = userId;
            dashboard.CreatedOn = DateTime.Now;
        }, ct);
    }

    public async Task<DashboardBuildDTO> Update(DashboardBuildDTO build, CancellationToken ct)
    {
        await ValidateDashboard(build, ct);

        // Update dashboard.
        var dashboard = await _dataService.Update<DbModel.Dashboard, DashboardBuildDTO, Guid>(build, ct);

        // Delete all widgets of this dashboard that no longer belongs to it.
        var widgetIds = build.Widgets.Select(widget => widget.Id);
        await _dataService.DeleteAll<DashboardWidget>(query =>
            query.Where(widget => widget.DashboardId == build.Id && !widgetIds.Contains(widget.Id)), ct);

        // Release every widget corresponding to this dashboard widgets.
        foreach (var buildWidgetGrouping in build.Widgets.GroupBy(widget => widget.WidgetSourceId))
        {
            var provider = _widgetProviderFactory.GetWidgetProvider(buildWidgetGrouping.Key);
            if (provider == null) continue;

            // Release this widget source and update all the dashboard widgets that used it.
            var widgetSourceId = await provider.ReleaseDraft(buildWidgetGrouping.Key, ct);
            foreach (var buildWidget in buildWidgetGrouping) buildWidget.WidgetSourceId = widgetSourceId;
        }

        // Create or update dashboard widgets and assign them to this dashboard.
        foreach (var buildWidget in build.Widgets)
        {
            var widget = buildWidget.Id == 0
                ? await _dataService.Create<DashboardWidget, DashboardBuildWidgetDTO>(buildWidget,
                    beforeSave: (dashboardWidget, _) => { dashboardWidget.DashboardId = dashboard.Id; }, ct)
                : await _dataService.Update<DashboardWidget, DashboardBuildWidgetDTO>(buildWidget,
                    beforeSave: (dashboardWidget, _) => { dashboardWidget.DashboardId = dashboard.Id; }, ct);
            dashboard.Widgets.Add(widget);
        }

        return dashboard;
    }

    private async Task ValidateDashboard(DashboardBuildDTO dashboard, CancellationToken ct)
    {
        dashboard.Name = dashboard.Name.Trim();
        ValidateDashboardName(dashboard.Name);

        if (!string.IsNullOrEmpty(dashboard.UrlSlug))
        {
            dashboard.UrlSlug = dashboard.UrlSlug.Trim();
            await ValidateUrlSlugUnique(dashboard.UrlSlug, dashboard.Id != Guid.Empty ? dashboard.Id : null, ct);
        }
    }

    private static void ValidateDashboardName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessException("Dashboard name cannot be empty.");
        if (name.Length > 500) throw new BusinessException("Dashboard name cannot be longer than 500 characters.");
    }

    private async Task ValidateUrlSlugUnique(string urlSlug, Guid? excludeId, CancellationToken ct)
    {
        var unique = await _dataService.Context.Set<DbModel.Dashboard>().AllAsync(dashboard =>
            dashboard.UrlSlug == null || dashboard.Id == excludeId || !EF.Functions.Like(urlSlug, dashboard.UrlSlug), ct);

        if (!unique) throw new BusinessException($"Another dashboard with the same URL slug '{urlSlug}' already exists.");
    }
}