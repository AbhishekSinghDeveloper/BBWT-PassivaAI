using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Chart.DbModel;
using BBF.Reporting.Widget.Chart.DTO;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.Chart.Services;

public class WidgetChartViewService : IWidgetChartViewService
{
    private readonly IDataService _dataService;
    private readonly IQueryProviderFactory _qpFactory;

    public WidgetChartViewService(IDataService dataService, IQueryProviderFactory qpFactory)
    {
        _qpFactory = qpFactory;
        _dataService = dataService;
    }

    public async Task<ChartViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
    {
        var chart = await _dataService.Get<WidgetChart, ChartViewDTO>(query => query
            .Where(chart => widgetSourceId == chart.WidgetSourceId)
            .Include(chart => chart.Columns)
            .Include(chart => chart.WidgetSource)
            .ThenInclude(source => source.DisplayRule), ct);

        if (chart.QuerySourceId == null) return chart;

        var provider = _qpFactory.GetQueryProvider(chart.QuerySourceId.Value);
        if (provider == null)
            throw new BusinessException("Cannot find query provider for this query ID");

        chart.QueryVariables = await provider.GetQueryVariables(chart.QuerySourceId.Value, ct);

        return chart;
    }
}