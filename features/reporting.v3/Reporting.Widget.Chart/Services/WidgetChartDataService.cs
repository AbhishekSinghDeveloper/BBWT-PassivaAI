using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBWM.Core.Exceptions;

namespace BBF.Reporting.Widget.Chart.Services;

public class WidgetChartDataService : IWidgetChartDataService
{
    private readonly IQueryProviderFactory _qpFactory;

    public WidgetChartDataService(IQueryProviderFactory qpFactory)
        => _qpFactory = qpFactory;

    public Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);
        if (provider == null)
            throw new BusinessException("Cannot find query provider for this query ID");

        return provider.GetQuerySchema(querySourceId, ct);
    }

    public Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId, QueryVariables queryVariables,
        CancellationToken ct = default)
    {
        var provider = _qpFactory.GetQueryProvider(querySourceId);
        if (provider == null)
            throw new BusinessException("Cannot find query provider for this query ID");

        return provider.GetQueryDataRows(querySourceId, queryVariables, ct: ct);
    }

    public async Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = _qpFactory.GetMetadataProvider(querySourceId);
        if (provider == null)
            throw new BusinessException("Cannot find query metadata provider for this query ID");

        return await provider.GetViewMetadata(querySourceId, ct);
    }
}