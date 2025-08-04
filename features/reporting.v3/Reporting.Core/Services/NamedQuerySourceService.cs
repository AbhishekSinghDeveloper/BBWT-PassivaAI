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

public class NamedQuerySourceService : INamedQuerySourceService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;

    public NamedQuerySourceService(IDataService dataService, ILoggedUserService loggedUserService)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
    }

    private IQueryable<QuerySource> GetEntityQuery(IQueryable<QuerySource> baseQuery)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to his own query sources
        // and the queries published to his organization.
        return baseQuery.Where(UserHasAccessToQuerySourcePredicate(systemAdmin))
            .Where(querySource => !string.IsNullOrEmpty(querySource.Name) && !querySource.IsDraft)
            .Include(querySource => querySource.Owner)
            .Include(querySource => querySource.Organizations);
    }

    public async Task<bool> UserHasAccessToQuerySource(Guid querySourceId, CancellationToken ct = default)
    {
        // Check if this user is system admin.
        var systemAdmin = _loggedUserService.IsSystemAdmin();

        // If user is not system admin, then he can access only to its own query sources
        // and the query sources published to his organization.
        return await _dataService.Context.Set<QuerySource>()
            .Where(querySource => querySource.Id == querySourceId)
            .AnyAsync(UserHasAccessToQuerySourcePredicate(systemAdmin), ct);
    }

    // Returns a predicate that returns true only if the user is system admin or
    // this query source is published to one of his organizations.
    private Expression<Func<QuerySource, bool>> UserHasAccessToQuerySourcePredicate(bool systemAdmin)
    {
        var userId = _loggedUserService.GetLoggedUserId();
        if (userId == null) throw new BusinessException("Cannot get user Id");

        if (systemAdmin) return querySource => true;

        return querySource => querySource.OwnerId == userId || querySource.Organizations
            .Any(organization => organization.UserOrganizations.Any(user => user.UserId == userId));
    }

    public async Task<IEnumerable<QuerySourceDTO>> GetAll(CancellationToken ct = default)
        => await _dataService.GetAll<QuerySource, QuerySourceDTO>(GetEntityQuery, ct);

    public async Task<PageResult<QuerySourceDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
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

        return await _dataService.GetPage<QuerySource, QuerySourceDTO>(command, GetEntityQuery, ct);
    }

    public async Task ChangeOwner(Guid querySourceId, string userId, CancellationToken ct = default)
    {
        var querySource = await _dataService.Context.Set<QuerySource>()
            .FirstOrDefaultAsync(query => query.Id == querySourceId, ct);
        if (querySource == null) throw new BusinessException("There is no query with given id");

        var userExists = await _dataService.Context.Set<User>().AnyAsync(user => user.Id == userId, ct);
        if (!userExists) throw new BusinessException("There is no user with given id");

        querySource.OwnerId = userId;

        await _dataService.Context.SaveChangesAsync(ct);
    }

    public async Task PublishQuery(Guid querySourceId, IEnumerable<int> organizationIds, CancellationToken ct = default)
    {
        var querySource = await _dataService.Context.Set<QuerySource>()
            .Include(querySource => querySource.Organizations)
            .ThenInclude(organization => organization.UserOrganizations)
            .FirstOrDefaultAsync(querySource => querySource.Id == querySourceId, ct);

        if (querySource == null) throw new BusinessException("There is no query with given id.");
        if (string.IsNullOrEmpty(querySource.Name)) throw new BusinessException("Cannot publish a local query.");

        var userId = _loggedUserService.GetLoggedUserId();
        var isSystemAdmin = _loggedUserService.IsSystemAdmin();
        if (userId == null && !isSystemAdmin) throw new BusinessException("Cannot get user id to publish to organizations.");

        var organizationIdsList = organizationIds.ToList();

        // Get all removed organizations (organizations that belongs to this user organizations and whose id is not present in the list).
        var removedOrganizations = querySource.Organizations.Where(organization =>
            (isSystemAdmin || organization.UserOrganizations.Any(user => user.UserId == userId))
            && !organizationIdsList.Contains(organization.Id)).ToList();

        // Remove all those organizations from the query source entity.
        if (removedOrganizations.Count > 0)
        {
            foreach (var organizationId in removedOrganizations)
                querySource.Organizations.Remove(organizationId);
        }

        // Get all new organization ids.
        var newOrganizationIds = organizationIdsList.Where(organizationId =>
            querySource.Organizations.All(organization => organization.Id != organizationId)).ToList();

        // Add all those organizations to the query source entity.
        if (newOrganizationIds.Count > 0)
        {
            var newOrganizations = await _dataService.Context.Set<Organization>()
                .Where(organization => newOrganizationIds.Contains(organization.Id)).ToListAsync(ct);

            foreach (var organization in newOrganizations)
                querySource.Organizations.Add(organization);
        }

        // Save tge changes.
        await _dataService.Context.SaveChangesAsync(ct);
    }
}