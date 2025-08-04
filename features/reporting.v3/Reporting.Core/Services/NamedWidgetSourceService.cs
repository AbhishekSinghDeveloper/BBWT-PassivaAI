using System.Linq.Expressions;
using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Core.Services;

public class NamedWidgetSourceService : INamedWidgetSourceService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;

    public NamedWidgetSourceService(IDataService dataService, ILoggedUserService loggedUserService)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
    }

    private IQueryable<WidgetSource> GetEntityQuery(IQueryable<WidgetSource> baseQuery)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own widget sources
        // and the queries published to his organization.
        return baseQuery.Where(UserHasAccessToWidgetSourcePredicate(systemAdmin))
            .Where(widgetSource => !string.IsNullOrEmpty(widgetSource.Name) && !widgetSource.IsDraft)
            .Include(widgetSource => widgetSource.Owner)
            .Include(widgetSource => widgetSource.Organizations);
    }

    public async Task<bool> UserHasAccessToWidgetSource(Guid widgetSourceId, CancellationToken ct = default)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to its own widget sources
        // and the widget sources published to his organization.
        return await _dataService.Context.Set<WidgetSource>()
            .Where(widgetSource => widgetSource.Id == widgetSourceId)
            .AnyAsync(UserHasAccessToWidgetSourcePredicate(systemAdmin), ct);
    }

    public async Task<bool> UserHasAccessToWidgetSource(string widgetCode, CancellationToken ct = default)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own dashboards
        // and the dashboards published to his organization.
        return await _dataService.Context.Set<WidgetSource>()
            .Where(widgetSource => widgetSource.Code == widgetCode)
            .AnyAsync(UserHasAccessToWidgetSourcePredicate(systemAdmin), ct);
    }

    // Returns a predicate that returns true only if the user is system admin or
    // this widget source is published to one of his organizations.
    private Expression<Func<WidgetSource, bool>> UserHasAccessToWidgetSourcePredicate(bool systemAdmin)
    {
        var userId = _loggedUserService.GetLoggedUserId();
        if (userId == null) throw new BusinessException("Cannot get user Id");

        if (systemAdmin) return widgetSource => true;

        return widgetSource => widgetSource.OwnerId == userId || widgetSource.Organizations
            .Any(organization => organization.UserOrganizations.Any(user => user.UserId == userId));
    }

    public async Task<IEnumerable<WidgetSourceDTO>> GetAll(CancellationToken ct = default)
        => await _dataService.GetAll<WidgetSource, WidgetSourceDTO>(GetEntityQuery, ct);

    public Task<WidgetSourcePreloadDTO> GetByCode(string widgetCode, CancellationToken ct = default)
        => _dataService.Get<WidgetSource, WidgetSourcePreloadDTO>(query =>
            GetEntityQuery(query).Where(widgetSource => widgetSource.Code == widgetCode), ct);

    public async Task<PageResult<WidgetSourceDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
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

        return await _dataService.GetPage<WidgetSource, WidgetSourceDTO>(command, GetEntityQuery, ct);
    }

    public async Task ChangeOwner(Guid widgetSourceId, string userId, CancellationToken ct = default)
    {
        var widgetSource = await _dataService.Context.Set<WidgetSource>()
            .FirstOrDefaultAsync(query => query.Id == widgetSourceId, ct);
        if (widgetSource == null) throw new BusinessException("There is no widget with given id");

        var userExists = await _dataService.Context.Set<User>().AnyAsync(user => user.Id == userId, ct);
        if (!userExists) throw new BusinessException("There is no user with given id");

        widgetSource.OwnerId = userId;

        await _dataService.Context.SaveChangesAsync(ct);
    }

    public async Task PublishWidget(Guid widgetSourceId, IEnumerable<int> organizationIds, CancellationToken ct = default)
    {
        var widgetSource = await _dataService.Context.Set<WidgetSource>()
            .Include(widgetSource => widgetSource.Organizations)
            .ThenInclude(organization => organization.UserOrganizations)
            .FirstOrDefaultAsync(widgetSource => widgetSource.Id == widgetSourceId, ct);

        if (widgetSource == null) throw new BusinessException("There is no widget with given id.");
        if (string.IsNullOrEmpty(widgetSource.Name)) throw new BusinessException("Cannot publish a local widget.");

        var userId = _loggedUserService.GetLoggedUserId();
        var isSystemAdmin = _loggedUserService.IsSystemAdmin();
        if (userId == null && !isSystemAdmin) throw new BusinessException("Cannot get user id to publish to organizations.");

        var organizationIdsList = organizationIds.ToList();

        // Get all removed organizations (organizations that belongs to this user organizations and whose id is not present in the list).
        var removedOrganizations = widgetSource.Organizations.Where(organization =>
            (isSystemAdmin || organization.UserOrganizations.Any(user => user.UserId == userId))
            && !organizationIdsList.Contains(organization.Id)).ToList();

        // Remove all those organizations from the widgetSource entity.
        if (removedOrganizations.Count > 0)
        {
            foreach (var organizationId in removedOrganizations)
                widgetSource.Organizations.Remove(organizationId);
        }

        // Get all new organization ids.
        var newOrganizationIds = organizationIdsList.Where(organizationId =>
            widgetSource.Organizations.All(organization => organization.Id != organizationId)).ToList();

        // Add all those organizations to the widget source entity.
        if (newOrganizationIds.Count > 0)
        {
            var newOrganizations = await _dataService.Context.Set<Organization>()
                .Where(organization => newOrganizationIds.Contains(organization.Id)).ToListAsync(ct);

            foreach (var organization in newOrganizations)
                widgetSource.Organizations.Add(organization);
        }

        // Save tge changes.
        await _dataService.Context.SaveChangesAsync(ct);
    }
}