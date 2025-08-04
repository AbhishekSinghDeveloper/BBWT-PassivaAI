using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Grid.DbModel;
using BBF.Reporting.Widget.Grid.DTO;
using BBF.Reporting.Widget.Grid.Enums;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace BBF.Reporting.Widget.Grid.Services;

public class WidgetGridBuilderService : IWidgetGridBuilderService
{
    private readonly IDataService _dataService;
    private readonly IWidgetGridProvider _widgetGridProvider;
    private readonly IWidgetSourceService _widgetSourceService;
    private readonly IQueryProviderFactory _queryProviderFactory;


    public WidgetGridBuilderService(
        IDataService dataService,
        IWidgetGridProvider widgetGridProvider,
        IWidgetSourceService widgetSourceService,
        IQueryProviderFactory queryProviderFactory)
    {
        _dataService = dataService;
        _widgetGridProvider = widgetGridProvider;
        _widgetSourceService = widgetSourceService;
        _queryProviderFactory = queryProviderFactory;
    }

    public async Task<GridViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
    {
        var grid = await _dataService.Get<WidgetGrid, GridViewDTO>(query => query
                    .Where(grid => grid.WidgetSourceId == widgetSourceId)
                    .Include(grid => grid.WidgetSource).ThenInclude(source => source.DisplayRule)
                    .Include(grid => grid.Columns).ThenInclude(column => column.Variable), ct)
                ?? throw new ObjectNotExistsException("The widget source with specified ID doesn't exist.");
        return grid;
    }

    public Task<GridViewDTO> Create(GridViewDTO build, CancellationToken ct = default)
        => Create(build, null, ct);

    public async Task<GridViewDTO> Create(GridViewDTO build, string? userId, CancellationToken ct = default)
    {
        // Create new source for this grid.
        const string widgetType = WidgetGridProvider.SourceType;
        var source = await _widgetSourceService.Create(build.WidgetSource, widgetType, userId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this grid widget");

        return await CreateGrid(source, build, ct);
    }

    public async Task<GridViewDTO> CreateDraft(GridViewDTO build, Guid? releaseWidgetId = null,
        CancellationToken ct = default)
    {
        // Create new source for this grid.
        const string widgetType = WidgetGridProvider.SourceType;
        var source = await _widgetSourceService.CreateDraft(build.WidgetSource, widgetType, releaseWidgetId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this grid widget");

        return await CreateGrid(source, build, ct);
    }

    private async Task<GridViewDTO> CreateGrid(WidgetSourceDTO source, GridViewDTO build,
        CancellationToken ct = default)
    {
        // If this widget is not a draft, release its associated query.
        if (!source.IsDraft && build.QuerySourceId != null)
            build.QuerySourceId = await _widgetGridProvider.ReleaseQueryDraft(build.QuerySourceId.Value, ct);

        // Restore missing columns.
        await RestoreColumns(build, ct);

        // Create grid and assign it to this source.
        build.Id = 0;
        var grid = await _dataService.Create<WidgetGrid, GridViewDTO>(build,
            beforeSave: (widgetGrid, _) => { widgetGrid.WidgetSourceId = source.Id; }, ct);

        // Create chart columns and assign them to this chart.
        foreach (var buildColumn in build.Columns)
        {
            buildColumn.Id = 0;
            var column = await _dataService.Create<WidgetGridColumn, GridViewColumnDTO>(buildColumn,
                beforeSave: (widgetGridColumn, _) => UpdateGridColumn(grid.Id, widgetGridColumn, buildColumn), ct);
            grid.Columns.Add(column);
        }

        return grid;
    }

    public Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
        => _widgetGridProvider.ReleaseDraft(widgetSourceId, ct);

    public async Task<GridViewDTO> Update(GridViewDTO build, CancellationToken ct = default)
    {
        // Update source.
        var source = await _widgetSourceService.Update(build.WidgetSource, ct);

        // If this widget is not a draft, release its associated query.
        if (!source.IsDraft && build.QuerySourceId != null)
            build.QuerySourceId = await _widgetGridProvider.ReleaseQueryDraft(build.QuerySourceId.Value, ct);

        // Restore missing columns.
        await RestoreColumns(build, ct);

        // Update grid.
        var grid = await _dataService.Update<WidgetGrid, GridViewDTO>(build, ct);

        // Delete all columns of this grid that no longer belongs to it.
        var columnIds = build.Columns.Select(column => column.Id);
        await _dataService.DeleteAll<WidgetGridColumn>(query =>
            query.Where(column => column.GridId == build.Id && !columnIds.Contains(column.Id)), ct);

        // Create or update chart columns and assign them to this chart.
        foreach (var buildColumn in build.Columns)
        {
            var column = buildColumn.Id == 0
                ? await _dataService.Create<WidgetGridColumn, GridViewColumnDTO>(buildColumn,
                    beforeSave: (widgetGridColumn, _) => UpdateGridColumn(grid.Id, widgetGridColumn, buildColumn), ct)
                : await _dataService.Update<WidgetGridColumn, GridViewColumnDTO>(buildColumn,
                    beforeSave: (widgetGridColumn, _) => UpdateGridColumn(grid.Id, widgetGridColumn, buildColumn), ct);
            grid.Columns.Add(column);
        }

        return grid;
    }

    private void UpdateGridColumn(int gridId, WidgetGridColumn column, GridViewColumnDTO buildColumn)
    {
        column.GridId = gridId;

        // Get associated variable if exists.
        column.Variable = column.VariableId != null ? _dataService.Context.Set<Variable>().Find(column.VariableId) : null;

        if (buildColumn.VariableName is { Length: > 0 } name)
        {
            column.Variable ??= new Variable();
            column.Variable.Name = name;
        }
        else if (column.Variable != null)
        {
            _dataService.Context.Entry(column.Variable).State = EntityState.Deleted;
        }
    }

    private async Task RestoreColumns(GridViewDTO build, CancellationToken ct = default)
    {
        var queryProvider = _queryProviderFactory.GetQueryProvider(build.QuerySourceId!.Value);
        if (queryProvider == null) throw new BusinessException("Cannot find query provider for this query ID");

        var querySchema = await queryProvider.GetQuerySchema(build.QuerySourceId!.Value, ct);

        var metadataProvider = _queryProviderFactory.GetMetadataProvider(build.QuerySourceId!.Value);

        ViewMetadata? viewMetadata = null;
        if (metadataProvider != null)
        {
            viewMetadata = await metadataProvider.GetViewMetadata(build.QuerySourceId!.Value, ct);
            viewMetadata.CustomColumnTypes = await metadataProvider.GetCustomColumnTypes(ct);
        }

        // Remove all columns that doesn't belong to query schema.
        build.Columns = build.Columns
            .Where(column => querySchema.Columns.Any(schemaColumn => column.QueryAlias == schemaColumn.QueryAlias)).ToList();

        // Get all missing columns from query schema.
        var missingColumns = querySchema.Columns
            .Where(schemaColumn => build.Columns.All(column => column.QueryAlias != schemaColumn.QueryAlias)).ToList();

        foreach (var schemaColumn in missingColumns)
        {
            var metadataColumn = viewMetadata?.Columns.FirstOrDefault(metadataColumn => metadataColumn.QueryAlias == schemaColumn.QueryAlias);
            AddWidgetGridColumn(build, schemaColumn, metadataColumn);
        }
    }

    private static void AddWidgetGridColumn(GridViewDTO build, QuerySchemaColumn querySchemaColumn,
        ViewMetadataColumn? viewMetadataColumn = null)
    {
        var column = new GridViewColumnDTO
        {
            QueryAlias = querySchemaColumn.QueryAlias,
            Header = viewMetadataColumn?.Title ?? querySchemaColumn.QueryAlias,
            DataType = querySchemaColumn.DataType,
            Sortable = true,
            Visible = true
        };

        column.DisplayMode = GetDisplayModeByDataType(column.DataType);
        column.InputType = GetDefaultInputTypeByDataType(column.DataType);
        column.SortOrder = build.Columns.Count;
        build.Columns.Add(column);
    }

    private static DisplayMode GetDisplayModeByDataType(DataType dataType)
        => dataType switch
        {
            DataType.Bool => DisplayMode.Conditional,
            DataType.Date => DisplayMode.Date,
            DataType.Numeric => DisplayMode.Number,
            _ => DisplayMode.Text,
        };

    private static InputType GetDefaultInputTypeByDataType(DataType dataType)
        => dataType switch
        {
            DataType.Bool => InputType.Checkbox,
            DataType.Date => InputType.Calendar,
            DataType.Numeric => InputType.Number,
            _ => InputType.Text,
        };
}