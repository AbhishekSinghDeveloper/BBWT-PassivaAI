using BBF.Reporting.Core.Model;
using BBF.Reporting.QueryBuilder.Interfaces;

namespace BBF.Reporting.QueryBuilder.Services;

/// <summary>
/// For now, Automatic query builder only remains as a concept in code models in order to highlight
/// that we may have multiple query processing providers. If in future we implement automatic (interactive)
/// query builder, we may reactivate these classes
/// </summary>
public class AutoQuerySourceProvider : IAutoQuerySourceProvider
{
    public const string SourceType = "auto";

    public Task<IEnumerable<CustomColumnType>> GetCustomColumnTypes(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<dynamic> GetQueryDataAggregations(Guid querySourceId, IList<QueryColumnAggregation> aggregations, QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId, QueryVariables? queryVariables = null, PagedGridSettings? gridSettings = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetQueryDataRowsCount(Guid querySourceId, QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetQueryVariables(Guid querySourceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}