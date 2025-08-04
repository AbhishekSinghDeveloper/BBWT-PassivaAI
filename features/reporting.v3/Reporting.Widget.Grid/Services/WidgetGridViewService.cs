using AutoMapper;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Widget.Grid.DbModel;
using BBF.Reporting.Widget.Grid.DTO;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;

namespace BBF.Reporting.Widget.Grid.Services;

public class WidgetGridViewService : IWidgetGridViewService
{
    private readonly IDbContext _context;
    private readonly IQueryProviderFactory _qpFactory;
    private readonly IMapper _mapper;


    public WidgetGridViewService(
        IMapper mapper,
        IDbContext context,
        IQueryProviderFactory qpFactory)
    {
        _context = context;
        _qpFactory = qpFactory;
        _mapper = mapper;
    }

    public async Task<GridDisplayViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
    {
        var widgetGrid = await _context.Set<WidgetGrid>()
                             .Include(grid => grid.WidgetSource).ThenInclude(source => source.DisplayRule)
                             .Include(grid => grid.Columns).ThenInclude(column => column.Variable)
                             .FirstOrDefaultAsync(grid => grid.WidgetSourceId == widgetSourceId, ct)
                         ?? throw new ObjectNotExistsException("The widget source with specified ID doesn't exist.");

        var grid = _mapper.Map<GridDisplayViewDTO>(widgetGrid);

        ViewMetadata? viewMetadata = null;
        var metadataProvider = _qpFactory.GetMetadataProvider((Guid)widgetGrid.QuerySourceId!);
        if (metadataProvider != null)
        {
            viewMetadata = await metadataProvider.GetViewMetadata((Guid)widgetGrid.QuerySourceId!, default);
            viewMetadata.CustomColumnTypes = await metadataProvider.GetCustomColumnTypes(ct);
        }

        HandleDisplayView(grid, widgetGrid, viewMetadata);

        if (widgetGrid.QuerySourceId == null) return grid;

        var querySourceId = widgetGrid.QuerySourceId.Value;
        grid.QueryVariables = await _qpFactory.GetQueryProvider(querySourceId)!.GetQueryVariables(querySourceId, ct);

        return grid;
    }


    private void HandleDisplayView(GridDisplayViewDTO displayView, WidgetGrid widgetGrid, ViewMetadata? viewMetadata)
    {
        foreach (var widgetGridColumn in widgetGrid.Columns)
        {
            var displayViewColumn =
                displayView.Columns.Single(column => column.QueryAlias == widgetGridColumn.QueryAlias);
            var metadataColumn =
                viewMetadata?.Columns.Single(column => column.QueryAlias == widgetGridColumn.QueryAlias);
            var widgetGridExtraSettingsJsonNode = JsonNode.Parse(widgetGridColumn.ExtraSettings ?? "{}")!;
            var maskDisplayMode = widgetGridExtraSettingsJsonNode["maskDisplayMode"]?.GetValue<int>();
            var widthMode = widgetGridExtraSettingsJsonNode["widthMode"]?.GetValue<int>();

            if (widgetGridColumn.InheritHeader) displayViewColumn.Header = metadataColumn?.Title;

            if (maskDisplayMode == 0) displayViewColumn.ExtraSettings["mask"] = metadataColumn?.Mask;

            if (widthMode == 0)
            {
                displayViewColumn.ExtraSettings["width"] = metadataColumn?.Width;
                displayViewColumn.ExtraSettings["minWidth"] = metadataColumn?.MinWidth;
                displayViewColumn.ExtraSettings["maxWidth"] = metadataColumn?.MaxWidth;
            }

            if (maskDisplayMode != 1 || widgetGridColumn.CustomColumnTypeId == null) continue;

            var customColumnType = viewMetadata?.CustomColumnTypes
                .SingleOrDefault(type => type.Id == widgetGridColumn.CustomColumnTypeId);

            if (customColumnType != null) displayViewColumn.ExtraSettings["mask"] = customColumnType.Mask;
        }

        displayView.Columns = displayView.Columns.OrderBy(column => column.SortOrder).ToList();
    }
}