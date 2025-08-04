using BBF.Reporting.Core.Model;

namespace BBF.Reporting.Widget.Chart.Interfaces;

public interface IWidgetChartDataService
{
    Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct = default);

    Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId, QueryVariables queryVariables,
        CancellationToken ct = default);

    Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct = default);
}