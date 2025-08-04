using BBF.Reporting.QueryBuilder.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbService :
    IEntityCreate<SqlQueryBuildDTO>,
    IEntityUpdate<SqlQueryBuildDTO>
{
    Task<SqlQueryBuildDTO> GetBuild(Guid querySourceId, CancellationToken ct);

    Task<SqlQueryBuildDTO> Create(SqlQueryBuildDTO queryBuild, string userId, CancellationToken ct = default);

    Task<SqlQueryBuildDTO> CreateDraft(SqlQueryBuildDTO queryBuild, Guid? releaseQueryId = null,
        CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default);
}