using BBF.Reporting.Core.Model;

namespace BBF.Reporting.Core.Interfaces;

public interface IQuerySourceProvider
{
    Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct);

    Task<IEnumerable<string>> GetQueryVariables(Guid querySourceId, CancellationToken ct);

    Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId,
        QueryVariables? queryVariables = null,
        PagedGridSettings? gridSettings = null, CancellationToken ct = default);

    Task<int> GetQueryDataRowsCount(Guid querySourceId, QueryVariables? queryVariables = null,
        CancellationToken ct = default);

    Task<dynamic> GetQueryDataAggregations(Guid querySourceId, IList<QueryColumnAggregation> aggregations,
        QueryVariables? queryVariables = null, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default);
}