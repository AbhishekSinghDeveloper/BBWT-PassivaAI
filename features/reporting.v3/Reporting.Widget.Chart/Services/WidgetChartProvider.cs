using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Chart.DbModel;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.Chart.Services;

public class WidgetChartProvider : IWidgetChartProvider
{
    public const string SourceType = "chart";

    private readonly IDbContext _context;
    private readonly IWidgetSourceService _widgetSourceService;
    private readonly IQueryProviderFactory _queryProviderFactory;

    public WidgetChartProvider(IDbContext context,
        IWidgetSourceService widgetSourceService,
        IQueryProviderFactory queryProviderFactory)
    {
        _context = context;
        _widgetSourceService = widgetSourceService;
        _queryProviderFactory = queryProviderFactory;
    }

    public async Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => await _context.Set<WidgetChart>()
            .AnyAsync(chart => chart.QuerySourceId == querySourceId, ct);

    public async Task<IEnumerable<WidgetSource>> GetAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => await _context.Set<WidgetChart>()
            .Where(chart => chart.QuerySourceId == querySourceId)
            .Select(chart => chart.WidgetSource).ToListAsync(ct);

    public Task<Guid> ReleaseQueryDraft(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = _queryProviderFactory.GetQueryProvider(querySourceId);
        if (provider == null) throw new BusinessException("Cannot find query provider for this query ID");

        return provider.ReleaseDraft(querySourceId, ct);
    }

    public async Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
    {
        var draftChart = await _context.Set<WidgetChart>()
                             .Include(chart => chart.Columns)
                             .FirstOrDefaultAsync(chart => chart.WidgetSourceId == widgetSourceId, ct)
                         ?? throw new ObjectNotExistsException("Draft chart widget with specified ID doesn't exist.");

        // Release this widget's query.
        if (draftChart.QuerySourceId != null)
            draftChart.QuerySourceId = await ReleaseQueryDraft(draftChart.QuerySourceId.Value, ct);

        var releasedChartId = await _widgetSourceService.ReleaseDraft(widgetSourceId, ct);
        if (releasedChartId == widgetSourceId) return widgetSourceId;

        var releaseChart = await _context.Set<WidgetChart>()
                               .Include(chart => chart.Columns)
                               .FirstOrDefaultAsync(chart => chart.WidgetSourceId == releasedChartId, ct)
                           ?? throw new ObjectNotExistsException("Released chart widget with specified ID doesn't exist.");

        // Copy edited draft fields to released chart.
        releaseChart.ChartSettingsJson = draftChart.ChartSettingsJson;

        releaseChart.QuerySourceId = draftChart.QuerySourceId;
        releaseChart.QuerySource = draftChart.QuerySource;

        releaseChart.Columns = (from column in draftChart.Columns
            select new WidgetChartColumn
            {
                QueryAlias = column.QueryAlias,
                ChartAlias = column.ChartAlias,
                ColumnPurpose = column.ColumnPurpose,

                ChartId = releaseChart.Id,

                Chart = releaseChart
            }).ToList();

        await _context.SaveChangesAsync(ct);

        return releasedChartId;
    }
}