using BBF.Reporting.Core.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Core.Interfaces;

public interface IQuerySourceService :
    IEntityCreate<QuerySourceDTO>,
    IEntityUpdate<QuerySourceDTO>,
    IEntityDelete<Guid>
{
    Task<QuerySourceDTO> Create(QuerySourceDTO dto, string queryType, string? ownerId = null,
        CancellationToken ct = default);

    Task<QuerySourceDTO> CreateDraft(QuerySourceDTO dto, string queryType, Guid? releaseQueryId = null,
        CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default);

    Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default);
}