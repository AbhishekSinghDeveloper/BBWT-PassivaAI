using System.Linq.Expressions;
using BBF.Reporting.Dashboard.Interfaces;
using BBF.Reporting.Dashboard.DTO;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Filters.TypedFilters;

namespace BBF.Reporting.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;

    public DashboardService(IDataService dataService, ILoggedUserService loggedUserService)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
    }

    private IQueryable<DbModel.Dashboard> GetEntityQuery(IQueryable<DbModel.Dashboard> baseQuery)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own dashboards
        // and the dashboards published to his organization.
        return baseQuery.Where(UserHasAccessToDashboardPredicate(systemAdmin))
            .Include(dashboard => dashboard.Owner)
            .Include(dashboard => dashboard.Organizations)
            .Include(dashboard => dashboard.Widgets)
            .ThenInclude(widget => widget.WidgetSource);
    }

    public async Task<bool> UserHasAccessToDashboard(Guid dashboardId, CancellationToken ct = default)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own dashboards
        // and the dashboards published to his organization.
        return await _dataService.Context.Set<DbModel.Dashboard>()
            .Where(dashboard => dashboard.Id == dashboardId)
            .AnyAsync(UserHasAccessToDashboardPredicate(systemAdmin), ct);
    }

    public async Task<bool> UserHasAccessToDashboard(string urlSlug, CancellationToken ct = default)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own dashboards
        // and the dashboards published to his organization.
        return await _dataService.Context.Set<DbModel.Dashboard>()
            .Where(dashboard => dashboard.UrlSlug == urlSlug)
            .AnyAsync(UserHasAccessToDashboardPredicate(systemAdmin), ct);
    }

    // Returns a predicate that returns true only if the user is system admin or
    // this dashboard is published to one of his organizations.
    private Expression<Func<DbModel.Dashboard, bool>> UserHasAccessToDashboardPredicate(bool systemAdmin)
    {
        var userId = _loggedUserService.GetLoggedUserId();
        if (userId == null) throw new BusinessException("Cannot get user Id");

        if (systemAdmin) return dashboard => true;

        return dashboard => dashboard.OwnerId == userId || dashboard.Organizations
            .Any(organization => organization.UserOrganizations.Any(user => user.UserId == userId));
    }

    public Task<PageResult<DashboardDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
    {
        // Search for a filter using virtual field "Published".
        var publishingFilterIndex = command.Filters.FindIndex(filter =>
            string.Equals(filter.PropertyName, "Published", StringComparison.InvariantCultureIgnoreCase));

        // If filter exist and it's a boolean filter.
        if (publishingFilterIndex >= 0 && command.Filters[publishingFilterIndex] is BooleanFilter publishingFilter)
        {
            // If the filter has value, substitute the filter by a number filter with field "Organizations.Count" and the
            // filtering conditions "Organizations.Count > 0" or "Organizations.Count == 0" according to original filter value.
            if (publishingFilter.Value != null)
            {
                command.Filters[publishingFilterIndex] = new NumberFilter
                {
                    Value = 0,
                    PropertyName = "Organizations.Count",
                    MatchMode = publishingFilter.Value.Value ? CountableFilterMatchMode.GreaterThan : CountableFilterMatchMode.Equals
                };
            }
            // If the filter has no value, remove it.
            else command.Filters.RemoveAt(publishingFilterIndex);
        }

        return _dataService.GetPage<DbModel.Dashboard, DashboardDTO>(command, GetEntityQuery, ct);
    }

    public async Task ChangeOwner(Guid dashboardId, string userId, CancellationToken ct = default)
    {
        var dashboard = await _dataService.Context.Set<DbModel.Dashboard>()
            .FirstOrDefaultAsync(dashboard => dashboard.Id == dashboardId, ct);
        if (dashboard == null) throw new BusinessException("There is no dashboard with given id");

        var userExists = await _dataService.Context.Set<User>().AnyAsync(user => user.Id == userId, ct);
        if (!userExists) throw new BusinessException("There is no user with given id");

        dashboard.OwnerId = userId;

        await _dataService.Context.SaveChangesAsync(ct);
    }

    public async Task PublishDashboard(Guid dashboardId, IEnumerable<int> organizationIds, CancellationToken ct = default)
    {
        var dashboard = await _dataService.Context.Set<DbModel.Dashboard>()
            .Include(dashboard => dashboard.Organizations)
            .ThenInclude(organization => organization.UserOrganizations)
            .FirstOrDefaultAsync(dashboard => dashboard.Id == dashboardId, ct);

        if (dashboard == null) throw new BusinessException("There is no dashboard with given id.");

        var userId = _loggedUserService.GetLoggedUserId();
        var isSystemAdmin = _loggedUserService.IsSystemAdmin();
        if (userId == null && !isSystemAdmin) throw new BusinessException("Cannot get user id to publish to organizations.");

        var organizationIdsList = organizationIds.ToList();

        // Get all removed organizations (organizations that belongs to this user organizations and whose id is not present in the list).
        var removedOrganizations = dashboard.Organizations.Where(organization =>
            (isSystemAdmin || organization.UserOrganizations.Any(user => user.UserId == userId))
            && !organizationIdsList.Contains(organization.Id)).ToList();

        // Remove all those organizations from the dashboard entity.
        if (removedOrganizations.Count > 0)
        {
            foreach (var organizationId in removedOrganizations)
                dashboard.Organizations.Remove(organizationId);
        }

        // Get all new organization ids.
        var newOrganizationIds = organizationIdsList.Where(organizationId =>
            dashboard.Organizations.All(organization => organization.Id != organizationId)).ToList();

        // Add all those organizations to the dashboard entity.
        if (newOrganizationIds.Count > 0)
        {
            var newOrganizations = await _dataService.Context.Set<Organization>()
                .Where(organization => newOrganizationIds.Contains(organization.Id)).ToListAsync(ct);

            foreach (var organization in newOrganizations)
                dashboard.Organizations.Add(organization);
        }

        // Save tge changes.
        await _dataService.Context.SaveChangesAsync(ct);
    }

    public Task Delete(Guid id, CancellationToken ct = default)
        => _dataService.Delete<DbModel.Dashboard, Guid>(id, ct);
}