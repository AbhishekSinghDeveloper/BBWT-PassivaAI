using BBF.Reporting.Core.Model;

namespace BBF.Reporting.Widget.Grid.Interfaces;

public interface IWidgetGridDataService
{
    Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct = default);

    Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId, QueryPageRequest queryPageRequest,
        CancellationToken ct = default);

    Task<int> GetQueryDataRowsCount(Guid querySourceId, QueryVariables? queryVariables = null,
        CancellationToken ct = default);

    Task<dynamic> GetQueryDataAggregations(Guid querySourceId, IList<QueryColumnAggregation> aggregations,
        QueryVariables? queryVariables = null, CancellationToken ct = default);

    Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct = default);
}