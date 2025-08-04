using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Core.Services;

public class QuerySourceService : IQuerySourceService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IWidgetProviderFactory _widgetProviderFactory;

    public QuerySourceService(IDataService dataService,
        IWidgetProviderFactory widgetProviderFactory,
        ILoggedUserService loggedUserService)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
        _widgetProviderFactory = widgetProviderFactory;
    }

    public async Task<QuerySourceDTO> Create(QuerySourceDTO dto, CancellationToken ct = default)
    {
        await ValidateQuerySource(dto, ct);

        dto.Id = Guid.Empty;
        dto.CreatedOn = DateTime.Now;

        return await _dataService.Create<QuerySource, QuerySourceDTO>(dto, ct);
    }

    public Task<QuerySourceDTO> Create(QuerySourceDTO dto, string queryType, string? ownerId = null,
        CancellationToken ct = default)
    {
        dto.IsDraft = false;
        dto.ReleaseQueryId = null;
        dto.QueryType = queryType;
        dto.OwnerId = ownerId ?? GetLoggedUserId();

        return Create(dto, ct);
    }

    public async Task<QuerySourceDTO> CreateDraft(QuerySourceDTO dto, string queryType, Guid? releaseQueryId = null,
        CancellationToken ct = default)
    {
        dto.IsDraft = true;
        dto.QueryType = queryType;
        dto.OwnerId = GetLoggedUserId();
        dto.ReleaseQueryId = releaseQueryId;

        if (releaseQueryId == null) return await Create(dto, ct);

        // Search for this released query.
        var releaseQuery = await _dataService.Context.Set<QuerySource>()
            .FirstOrDefaultAsync(query => query.Id == releaseQueryId, ct);

        if (releaseQuery == null) throw new BusinessException("There is no released query with the given ID");
        if (releaseQuery.IsDraft) throw new BusinessException("Query source specified as release cannot be a draft.");

        return await Create(dto, ct);
    }

    public async Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default)
    {
        var draftSource = await _dataService.Context.Set<QuerySource>()
                              .FirstOrDefaultAsync(source => source.Id == querySourceId, ct)
                          ?? throw new ObjectNotExistsException("Query source with specified ID doesn't exist.");

        if (!draftSource.IsDraft) return querySourceId;
        var releaseQueryId = draftSource.ReleaseQueryId;

        // If there is no released version of this query, release this draft;
        if (releaseQueryId == null)
        {
            draftSource.IsDraft = false;
            await _dataService.Context.SaveChangesAsync(ct);
            return querySourceId;
        }

        // Otherwise, get released version of this query.
        var releaseSource = await _dataService.Context.Set<QuerySource>()
                                .FirstOrDefaultAsync(source => source.Id == releaseQueryId, ct)
                            ?? throw new ObjectNotExistsException("Query source with specified ID doesn't exist.");

        // Copy edited draft fields to released query.
        releaseSource.Name = draftSource.Name;
        await _dataService.Context.SaveChangesAsync(ct);

        return releaseQueryId.Value;
    }

    public async Task<QuerySourceDTO> Update(QuerySourceDTO dto, CancellationToken ct = default)
    {
        await ValidateQuerySource(dto, ct);

        return await _dataService.Update<QuerySource, QuerySourceDTO, Guid>(dto, beforeSave: (source, context) =>
        {
            var entry = context.Entry(source);
            var queryType = entry.OriginalValues.GetValue<string>(nameof(source.QueryType));

            // Prevent query type to change via update.
            dto.QueryType = queryType;
        }, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        // Delete all the drafts that have this query as released query.
        await DeleteDrafts(id, ct);

        // Delete this query.
        await _dataService.Delete<QuerySource, Guid>(id, ct);
    }

    private Task DeleteDrafts(Guid releaseQueryId, CancellationToken ct = default)
        => _dataService.DeleteAll<QuerySource>(query => query
            .Where(source => source.ReleaseQueryId == releaseQueryId), ct);

    public async Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
    {
        var widgetProviders = _widgetProviderFactory.GetWidgetProviders();
        foreach (var provider in widgetProviders.Where(provider => provider != null))
            if (await provider!.HasAttachedWidgets(querySourceId, ct))
                return true;

        return false;
    }

    private async Task ValidateQuerySource(QuerySourceDTO dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(dto.Name))
        {
            dto.Name = dto.Name.Trim();
            await ValidateQueryName(dto.Name, dto.IsDraft ? dto.ReleaseQueryId : dto.Id, ct);
        }
    }

    private async Task ValidateQueryName(string name, Guid? excludeId, CancellationToken ct)
    {
        if (name.Length > 500) throw new BusinessException("Query name cannot be longer than 500 characters.");

        var unique = await _dataService.Context.Set<QuerySource>().AllAsync(query =>
            query.Name == null || query.IsDraft || query.Id == excludeId || !EF.Functions.Like(name, query.Name), ct);

        if (!unique) throw new BusinessException($"Another query with the same name '{name}' already exists.");
    }

    private string GetLoggedUserId()
        => _loggedUserService.GetLoggedUserId()
           ?? throw new BusinessException("Cannot get user ID to set query source owner.");
}