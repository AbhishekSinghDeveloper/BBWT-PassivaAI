using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.QueryBuilder.DbModel;
using BBF.Reporting.QueryBuilder.DTO;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbService : IRqbService
{
    private readonly IDataService _dataService;
    private readonly IQuerySourceService _querySourceService;
    private readonly IRqbQuerySourceProvider _querySourceProvider;


    public RqbService(IDataService dataService,
        IRqbQuerySourceProvider querySourceProvider,
        IQuerySourceService querySourceService)
    {
        _dataService = dataService;
        _querySourceService = querySourceService;
        _querySourceProvider = querySourceProvider;
    }

    public async Task<SqlQueryBuildDTO> GetBuild(Guid querySourceId, CancellationToken ct = default)
        => await _dataService.Get<SqlQuery, SqlQueryBuildDTO>(query => query
            .Include(sqlQuery => sqlQuery.QuerySource)
            .Where(sqlQuery => sqlQuery.QuerySourceId == querySourceId), ct);

    public Task<SqlQueryBuildDTO> Create(SqlQueryBuildDTO build, CancellationToken ct = default)
        => Create(build, null, ct);

    public async Task<SqlQueryBuildDTO> Create(SqlQueryBuildDTO build, string? userId, CancellationToken ct = default)
    {
        await ValidateSqlCode(build.TableSetId, build.SqlCode, ct);

        const string sourceType = RqbQuerySourceProvider.SourceType;
        var source = await _querySourceService.Create(build.QuerySource, sourceType, userId, ct)
                     ?? throw new BusinessException("Cannot create query source for this sql query");

        return await CreateSqlQuery(source, build, ct);
    }

    public async Task<SqlQueryBuildDTO> CreateDraft(SqlQueryBuildDTO build, Guid? releaseQueryId = null,
        CancellationToken ct = default)
    {
        await ValidateSqlCode(build.TableSetId, build.SqlCode, ct);

        // Check compatibility with released query if necessary.
        if (releaseQueryId != null) await CheckQueryCompatibility(releaseQueryId.Value, build.SqlCode, ct);

        const string sourceType = RqbQuerySourceProvider.SourceType;
        var source = await _querySourceService.CreateDraft(build.QuerySource, sourceType, releaseQueryId, ct)
                     ?? throw new BusinessException("Cannot create query source for this sql query");

        return await CreateSqlQuery(source, build, ct);
    }

    private async Task<SqlQueryBuildDTO> CreateSqlQuery(QuerySourceDTO source, SqlQueryBuildDTO build,
        CancellationToken ct = default)
    {
        build.SqlCode = build.SqlCode.Trim();

        build.Id = 0;
        build.QuerySourceId = source.Id;

        return await _dataService.Create<SqlQuery, SqlQueryBuildDTO>(build, ct);
    }

    public Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default)
        => _querySourceProvider.ReleaseDraft(querySourceId, ct);

    public async Task<SqlQueryBuildDTO> Update(SqlQueryBuildDTO build, CancellationToken ct = default)
    {
        await ValidateSqlCode(build.TableSetId, build.SqlCode, ct);

        // Check compatibility with old version of the query.
        await CheckQueryCompatibility(build.QuerySourceId, build.SqlCode, ct);

        // Update source.
        await _querySourceService.Update(build.QuerySource, ct);

        build.SqlCode = build.SqlCode.Trim();

        return await _dataService.Update<SqlQuery, SqlQueryBuildDTO>(build, ct);
    }

    private async Task CheckQueryCompatibility(Guid querySourceId, string sqlCode, CancellationToken ct = default)
    {
        var query = await _dataService.Context.Set<SqlQuery>()
                        .Include(query => query.QuerySource).AsNoTracking()
                        .FirstOrDefaultAsync(query => query.QuerySourceId == querySourceId, ct)
                    ?? throw new BusinessException("There is no query with given ID");

        // If the query is not public, or no widget is using it, return.
        if (string.IsNullOrEmpty(query.QuerySource.Name) || !await _querySourceService.HasAttachedWidgets(querySourceId, ct)) return;

        // Otherwise, check compatibility of new query schema respect to the old one.
        var result = await _querySourceProvider.ValidateSchemaCompatibility(querySourceId, query.SqlCode, sqlCode, ct);
        if (!result.Valid)
            throw new BusinessException("The query cannot be updated because some widgets are using it and, " +
                                        "as this change causes the query schema to change, those widgets will break " +
                                        $"due to this modification.\nQuery schema change: {result.Message}");
    }

    private async Task ValidateSqlCode(int? tableSetId, string sqlCode, CancellationToken ct = default)
    {
        var result = await _querySourceProvider.ValidateSqlCode(tableSetId, sqlCode, ct);
        if (!result.Valid) throw new BusinessException(result.Message);
    }
}