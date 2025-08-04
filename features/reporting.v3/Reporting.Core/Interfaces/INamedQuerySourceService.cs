using BBF.Reporting.Core.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Core.Interfaces;

public interface INamedQuerySourceService :
    IEntityPage<QuerySourceDTO>
{
    Task<IEnumerable<QuerySourceDTO>> GetAll(CancellationToken ct = default);

    Task ChangeOwner(Guid querySourceId, string userId, CancellationToken ct = default);

    Task PublishQuery(Guid querySourceId, IEnumerable<int> organizationIds, CancellationToken ct = default);

    Task<bool> UserHasAccessToQuerySource(Guid querySourceId, CancellationToken ct = default);
}