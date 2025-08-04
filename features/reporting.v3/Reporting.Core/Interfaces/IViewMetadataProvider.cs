using BBF.Reporting.Core.Model;

namespace BBF.Reporting.Core.Interfaces;

public interface IViewMetadataProvider
{
    Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct);

    Task<IEnumerable<CustomColumnType>> GetCustomColumnTypes(CancellationToken ct = default);
}