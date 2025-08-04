using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Chart.DbModel;
using BBF.Reporting.Widget.Chart.DTO;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.Chart.Services;

public class WidgetChartBuilderService : IWidgetChartBuilderService
{
    private readonly IDataService _dataService;
    private readonly IWidgetChartProvider _widgetChartProvider;
    private readonly IWidgetSourceService _widgetSourceService;

    public WidgetChartBuilderService(IDataService dataService,
        IWidgetSourceService widgetSourceService,
        IWidgetChartProvider widgetChartProvider)
    {
        _dataService = dataService;
        _widgetSourceService = widgetSourceService;
        _widgetChartProvider = widgetChartProvider;
    }

    public Task<ChartBuildDTO> Create(ChartBuildDTO build, CancellationToken ct = default)
        => Create(build, null, ct);

    public async Task<ChartBuildDTO> Create(ChartBuildDTO build, string? userId, CancellationToken ct = default)
    {
        // Create new source for this chart.
        const string widgetType = WidgetChartProvider.SourceType;
        var source = await _widgetSourceService.Create(build.WidgetSource, widgetType, userId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this chart widget");

        return await CreateChart(source, build, ct);
    }

    public async Task<ChartBuildDTO> CreateDraft(ChartBuildDTO build, Guid? releaseWidgetId = null,
        CancellationToken ct = default)
    {
        // Create new source for this chart.
        const string widgetType = WidgetChartProvider.SourceType;
        var source = await _widgetSourceService.CreateDraft(build.WidgetSource, widgetType, releaseWidgetId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this chart widget");

        return await CreateChart(source, build, ct);
    }

    private async Task<ChartBuildDTO> CreateChart(WidgetSourceDTO source, ChartBuildDTO build,
        CancellationToken ct = default)
    {
        // If this widget is not a draft, release its associated query.
        if (!source.IsDraft && build.QuerySourceId != null)
            build.QuerySourceId = await _widgetChartProvider.ReleaseQueryDraft(build.QuerySourceId.Value, ct);

        // Create chart and assign it to this source.
        build.Id = 0;
        var chart = await _dataService.Create<WidgetChart, ChartBuildDTO>(build,
            beforeSave: (widgetChart, _) => { widgetChart.WidgetSourceId = source.Id; }, ct);

        // Create chart columns and assign them to this chart.
        foreach (var buildColumn in build.Columns)
        {
            buildColumn.Id = 0;
            var column = await _dataService.Create<WidgetChartColumn, ChartBuildColumnDTO>(buildColumn,
                beforeSave: (widgetChartColumn, _) => { widgetChartColumn.ChartId = chart.Id; }, ct);
            chart.Columns.Add(column);
        }

        return chart;
    }

    public Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
        => _widgetChartProvider.ReleaseDraft(widgetSourceId, ct);

    public async Task<ChartBuildDTO> Update(ChartBuildDTO build, CancellationToken ct = default)
    {
        // Update source.
        var source = await _widgetSourceService.Update(build.WidgetSource, ct);

        // If this widget is not a draft, release its associated query.
        if (!source.IsDraft && build.QuerySourceId != null)
            build.QuerySourceId = await _widgetChartProvider.ReleaseQueryDraft(build.QuerySourceId.Value, ct);

        // Update chart.
        var chart = await _dataService.Update<WidgetChart, ChartBuildDTO>(build, ct);

        // Delete all columns of this chart that no longer belongs to it.
        var columnIds = build.Columns.Select(column => column.Id);
        await _dataService.DeleteAll<WidgetChartColumn>(query =>
            query.Where(column => column.ChartId == build.Id && !columnIds.Contains(column.Id)), ct);

        // Create or update chart columns and assign them to this chart.
        foreach (var buildColumn in build.Columns)
        {
            var column = buildColumn.Id == 0
                ? await _dataService.Create<WidgetChartColumn, ChartBuildColumnDTO>(buildColumn,
                    beforeSave: (widgetChartColumn, _) => { widgetChartColumn.ChartId = chart.Id; }, ct)
                : await _dataService.Update<WidgetChartColumn, ChartBuildColumnDTO>(buildColumn,
                    beforeSave: (widgetChartColumn, _) => { widgetChartColumn.ChartId = chart.Id; }, ct);
            chart.Columns.Add(column);
        }

        return chart;
    }
}