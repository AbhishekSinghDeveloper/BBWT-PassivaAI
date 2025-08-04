using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Grid.DbModel;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.Grid.Services;

public class WidgetGridProvider : IWidgetGridProvider
{
    public const string SourceType = "table";

    private readonly IDbContext _context;
    private readonly IWidgetSourceService _widgetSourceService;
    private readonly IQueryProviderFactory _queryProviderFactory;

    public WidgetGridProvider(IDbContext context,
        IWidgetSourceService widgetSourceService,
        IQueryProviderFactory queryProviderFactory)
    {
        _context = context;
        _widgetSourceService = widgetSourceService;
        _queryProviderFactory = queryProviderFactory;
    }

    public async Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => await _context.Set<WidgetGrid>()
            .AnyAsync(grid => grid.QuerySourceId == querySourceId, ct);

    public async Task<IEnumerable<WidgetSource>> GetAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => await _context.Set<WidgetGrid>()
            .Where(grid => grid.QuerySourceId == querySourceId)
            .Select(grid => grid.WidgetSource).ToListAsync(ct);

    public Task<Guid> ReleaseQueryDraft(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = _queryProviderFactory.GetQueryProvider(querySourceId);
        if (provider == null) throw new BusinessException("Cannot find query provider for this query ID");

        return provider.ReleaseDraft(querySourceId, ct);
    }

    public async Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
    {
        var draftGrid = await _context.Set<WidgetGrid>()
                            .Include(grid => grid.Columns).ThenInclude(column => column.Variable)
                            .FirstOrDefaultAsync(grid => grid.WidgetSourceId == widgetSourceId, ct)
                        ?? throw new ObjectNotExistsException("Draft grid widget with specified ID doesn't exist.");

        // Release this widget's query.
        if (draftGrid.QuerySourceId != null)
            draftGrid.QuerySourceId = await ReleaseQueryDraft(draftGrid.QuerySourceId.Value, ct);

        var releasedGridId = await _widgetSourceService.ReleaseDraft(widgetSourceId, ct);
        if (releasedGridId == widgetSourceId) return widgetSourceId;

        var releaseGrid = await _context.Set<WidgetGrid>()
                              .Include(grid => grid.Columns).ThenInclude(column => column.Variable)
                              .FirstOrDefaultAsync(grid => grid.WidgetSourceId == releasedGridId, ct)
                          ?? throw new ObjectNotExistsException("Released grid widget with specified ID doesn't exist.");

        // Copy edited draft fields to released grid.
        releaseGrid.IsRowSelectable = draftGrid.IsRowSelectable;
        releaseGrid.SummaryFooterVisible = draftGrid.SummaryFooterVisible;
        releaseGrid.DefaultSortOrder = draftGrid.DefaultSortOrder;
        releaseGrid.DefaultSortColumnAlias = draftGrid.DefaultSortColumnAlias;
        releaseGrid.ShowVisibleColumnsSelector = draftGrid.ShowVisibleColumnsSelector;

        releaseGrid.QuerySourceId = draftGrid.QuerySourceId;
        releaseGrid.QuerySource = draftGrid.QuerySource;

        releaseGrid.Columns = (from column in draftGrid.Columns
            select new WidgetGridColumn
            {
                CustomColumnTypeId = column.CustomColumnTypeId,

                DataType = column.DataType,
                DisplayMode = column.DisplayMode,
                ExtraSettings = column.ExtraSettings,
                Footer = column.Footer,
                Header = column.Header,
                InheritHeader = column.InheritHeader,
                InputType = column.InputType,

                QueryAlias = column.QueryAlias,

                SortOrder = column.SortOrder,
                Sortable = column.Sortable,
                Visible = column.Visible,

                VariableId = column.VariableId,
                GridId = releaseGrid.Id,

                Variable = column.Variable,
                Grid = releaseGrid
            }).ToList();

        await _context.SaveChangesAsync(ct);

        return releasedGridId;
    }
}