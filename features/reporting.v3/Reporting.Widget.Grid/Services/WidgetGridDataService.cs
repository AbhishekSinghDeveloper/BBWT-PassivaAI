using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.Widget.Grid.Services;

public class WidgetGridDataService : IWidgetGridDataService
{
    private readonly IQueryProviderFactory _qpFactory;

    public WidgetGridDataService(IQueryProviderFactory qpFactory)
    {
        _qpFactory = qpFactory;
    }

    public async Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId,
        QueryPageRequest queryPageRequest, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);

        if (provider == null)
            throw new BusinessException("There is no query provider for the query associated with this grid.");

        return await provider.GetQueryDataRows(querySourceId,
            queryPageRequest.QueryVariables, queryPageRequest.GridSettings, ct);
    }

    public async Task<int> GetQueryDataRowsCount(Guid querySourceId,
        QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);

        if (provider == null)
            throw new BusinessException("There is no query provider for the query associated with this grid.");

        return await provider.GetQueryDataRowsCount(querySourceId, queryVariables, ct);
    }

    public async Task<dynamic> GetQueryDataAggregations(Guid querySourceId, IList<QueryColumnAggregation> aggregations,
        QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);

        if (provider == null)
            throw new BusinessException("There is no query provider for the query associated with this grid.");

        return await provider.GetQueryDataAggregations(querySourceId, aggregations, queryVariables, ct);
    }

    public async Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);

        if (provider == null)
            throw new BusinessException("There is no query provider for the query associated with this grid.");

        return await provider.GetQuerySchema(querySourceId, ct);
    }

    public async Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct = default)
    {
        ViewMetadata? metadata = null;
        var metadataProvider = _qpFactory.GetMetadataProvider(querySourceId);
        if (metadataProvider == null) return metadata ?? new ViewMetadata();

        metadata = await metadataProvider.GetViewMetadata(querySourceId, ct);
        metadata.CustomColumnTypes = await metadataProvider.GetCustomColumnTypes(ct);

        return metadata;
    }
}