using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using BBWM.Reporting.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BBWM.Reporting.Services
{
    public class ViewBuilderService : IViewBuilderService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataService _dataService;
        private readonly IDbDocService _dbDocService;
        private readonly IDbSchemaManager _dbSchemaManager;
        private readonly ILogger<ViewBuilderService> logger;

        public ViewBuilderService(
            IDbContext context,
            IMapper mapper,
            IDataService dataService,
            IDbDocService dbDocService,
            IDbSchemaManager dbSchemaManager,
            ILogger<ViewBuilderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _dataService = dataService;
            _dbDocService = dbDocService;
            _dbSchemaManager = dbSchemaManager;
            this.logger = logger;
        }


        public async Task<FilterControl> AddFilterControl(int viewId, FilterControlDTO dto, CancellationToken ct = default)
        {
            if (await _context.Set<View>().AllAsync(x => x.Id != viewId))
                throw new ObjectNotExistsException("The view with specified ID doesn't exist.");

            var filterControl = _mapper.Map<FilterControl>(dto);

            filterControl.ViewId = viewId;
            var otherFilters = _context.Set<FilterControl>().Where(x => x.ViewId == viewId);
            filterControl.SortOrder = await otherFilters.AnyAsync(ct) ? await otherFilters.MaxAsync(x => x.SortOrder, ct) + 1 : 1;

            await _context.Set<FilterControl>().AddAsync(filterControl, ct);

            await _context.SaveChangesAsync(ct);

            return filterControl;
        }

        public async Task<(GridViewColumn, IList<GridViewColumn>)> AddGridViewColumn(int gridViewId, int queryTableColumnId, Query relatedQuery, CancellationToken ct = default)
        {
            var gridViewColumn = await CreateGridViewColumn(gridViewId, queryTableColumnId, ct);
            await _context.SaveChangesAsync(ct);
            return gridViewColumn;
        }

        public async Task<IList<GridViewColumn>> AddGridViewColumns(int gridViewId, int queryTableId, CancellationToken ct = default)
        {
            var gridView = await _context.Set<GridView>()
                .Include(x => x.ViewColumns)
                .SingleOrDefaultAsync(x => x.Id == gridViewId, ct)
                ?? throw new ObjectNotExistsException("The table view with specified ID doesn't exist.");

            var queryTableColumnsWithoutViews = await _context.Set<QueryTableColumn>()
                .Where(x => x.QueryTableId == queryTableId && !gridView.ViewColumns.Select(y => y.QueryTableColumnId).Contains(x.Id))
                .ToListAsync(ct)
                ?? throw new ObjectNotExistsException("The query table with specified ID doesn't exist.");

            var addedGridViewColumns = new List<GridViewColumn>();
            foreach (var queryTableColumn in queryTableColumnsWithoutViews)
            {
                var createResult = await CreateGridViewColumn(gridView, queryTableColumn, false, ct);

                // Only passing the created column, ignoring affected columns as here we add columns to the bottom of
                // the list so columns sort order is not affected
                addedGridViewColumns.Add(createResult.Item1);
            }

            await _context.SaveChangesAsync(ct);

            return addedGridViewColumns;
        }

        public async Task<IEnumerable<QueryFilterBindingDTO>> BindFilterControlToNewQueryFilters(
            IEnumerable<QueryFilterBindingDTO> bindings,
            FilterControl filterControl,
            CancellationToken ct = default)
        {
            var result = new List<QueryFilterBinding>();
            foreach (var bindingDto in bindings.Where(o => o.QueryFilterId == 0))
            {
                var queryFilterDto = bindingDto.QueryFilter;
                var rootFilterSetId = _context.Set<View>()
                    .Where(x => x.Id == filterControl.ViewId)
                    .Select(x => x.Section.Query.RootFilterSet.Id)
                    .First();

                var binding = new QueryFilterBinding
                {
                    BindingType = QueryFilterBindingType.FilterControl,
                    FilterControl = filterControl,
                    QueryFilter = new QueryFilter
                    {
                        QueryFilterSetId = rootFilterSetId,
                        QueryTableColumnId = queryFilterDto.QueryTableColumnId,
                        QueryRuleId = queryFilterDto.QueryRuleId
                    }
                };

                await _context.Set<QueryFilterBinding>().AddAsync(binding, ct);
                result.Add(binding);
            }

            await _context.SaveChangesAsync(ct);

            return _mapper.Map<IEnumerable<QueryFilterBindingDTO>>(result);
        }

        /// <summary>
        /// </summary>
        /// <param name="overwriteBindings">
        /// When True, if filter control bindings for the query filter already exist,
        /// the previous bindings get replaced with the new filter control binding.
        /// When False, a new binding is added.
        /// </param>
        public async Task<QueryFilterBinding> BindFilterControlToQueryFilter(
            int filterControlId,
            int queryFilterId,
            CancellationToken ct = default)
        {
            var queryFilterBinding = await _context.Set<QueryFilterBinding>().FirstOrDefaultAsync(
                x => x.BindingType == QueryFilterBindingType.FilterControl && x.QueryFilterId == queryFilterId,
                ct);

            if (queryFilterBinding is not null)
            {
                if (queryFilterBinding.FilterControlId == filterControlId)
                    return queryFilterBinding;
            }
            else
            {
                queryFilterBinding = new QueryFilterBinding
                {
                    FilterControlId = filterControlId,
                    QueryFilterId = queryFilterId,
                    BindingType = QueryFilterBindingType.FilterControl
                };
            }

            var filterControl = await _context.Set<FilterControl>().SingleOrDefaultAsync(x => x.Id == filterControlId)
                ?? throw new ObjectNotExistsException("The filter control with specified ID doesn't exist.");

            var queryFilter = await _context.Set<QueryFilter>()
                .Include(x => x.QueryTableColumn).ThenInclude(x => x.QueryTable).ThenInclude(x => x.Query)
                .SingleOrDefaultAsync(x => x.Id == queryFilterId)
                ?? throw new ObjectNotExistsException("The query filter with specified ID doesn't exist.");

            var dbColumn = _dbSchemaManager.GetColumn(queryFilter.QueryTableColumn.SourceColumnId);
            if (!GetInputTypesSuitableForClrType(dbColumn.ClrTypeGroup).Contains(filterControl.InputType))
                throw new BusinessException("The filter control's input type is not suitable for the query filter.");

            if (queryFilterBinding.FilterControlId == filterControlId)
            {
                await _context.Set<QueryFilterBinding>().AddAsync(queryFilterBinding, ct);
            }
            else
            {
                queryFilterBinding.FilterControlId = filterControlId;
            }

            await _context.SaveChangesAsync(ct);

            return queryFilterBinding;
        }

        public async Task DeleteFilterControl(int filterControlId, bool deleteLinkedQueryFilters, CancellationToken ct = default)
        {
            var filterControl = await _context.Set<FilterControl>()
                .Include(o => o.QueryFilterBindings).ThenInclude(o => o.QueryFilter)
                .FirstOrDefaultAsync(o => o.Id.Equals(filterControlId), ct)
                ?? throw new ObjectNotExistsException("The filter control with specified ID doesn't exist.");

            foreach (var x in filterControl.QueryFilterBindings)
            {
                if (deleteLinkedQueryFilters)
                {
                    _context.Set<QueryFilter>().Remove(x.QueryFilter);
                }
                else
                {
                    _context.Set<QueryFilterBinding>().Remove(x);
                }
            }

            _context.Set<FilterControl>().Remove(filterControl);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteGridViewColumn(int gridViewColumnId, CancellationToken ct = default)
        {
            if (await _context.Set<GridViewColumn>().AllAsync(x => x.Id != gridViewColumnId))
                throw new ObjectNotExistsException("The table view column with specified ID doesn't exist.");

            await _dataService.Delete<GridViewColumn>(gridViewColumnId, ct);
        }

        public async Task DeleteQueryFilterBinding(int filterControlBindingId, CancellationToken ct = default)
        {
            var filterControlBinding = await _context.Set<QueryFilterBinding>()
                .SingleOrDefaultAsync(x => x.Id == filterControlBindingId, ct)
                ?? throw new ObjectNotExistsException("The filter control binding with specified ID doesn't exist.");

            _context.Set<QueryFilterBinding>().Remove(filterControlBinding);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<SectionDisplayViewDTO> GetSectionView(Section section, CancellationToken ct = default)
        {
            var result = new SectionDisplayViewDTO
            {
                AutoCollapse = section.AutoCollapse,
                Title = section.Title,
                ExpandBehaviour = section.ExpandBehaviour,
                DataViewType = section.DataViewType,
                Description = section.Description,
                ShowVisibleColumnsSelector = section.View.GridView.ShowVisibleColumnsSelector,
                SummaryFooterVisible = section.View.GridView.SummaryFooterVisible,
                DefaultSortOrder = section.View.GridView.DefaultSortOrder ?? SortOrder.Asc,
                Columns = new List<SectionViewColumnDTO>(),
                Filters = new List<SectionViewFilterDTO>(),
                MasterSectionEmittedEvents = new List<MasterSectionEmitEventType>(),
                MasterSectionBindings = new List<MasterSectionBindingDTO>(),
            };

            foreach (var viewColumn in section.View.GridView.ViewColumns.OrderBy(x => x.SortOrder))
            {
                //TODO: hack for demo
                var columnMetadata = section.Query.DbDocFolderId == null || viewColumn.QueryTableColumn.QueryTable.SourceCode == "form" ?
                    new DbDoc.DTO.ColumnMetadataDTO
                    {
                        StaticData = new DbSchemaColumn
                        {
                            ColumnName = viewColumn.QueryTableColumn.SourceColumnId,
                            ParentTableName = viewColumn.QueryTableColumn.QueryTable.SourceTableId
                        }
                    }
                    : (await _dbDocService.GetColumnMetadata(
                        section.Query.DbDocFolderId.Value, viewColumn.QueryTableColumn.SourceColumnId, ct)
                        ?? throw new ConflictException($"Column metadata '{viewColumn.QueryTableColumn.SourceColumnId}' not found for the view column"));

                var viewColumnDto = _mapper.Map<GridViewColumnDTO>(viewColumn);

                var sectionViewColumn = new SectionViewColumnDTO
                {
                    TableAlias = viewColumn.QueryTableColumn.QueryTable.Alias,
                    SortOrder = viewColumnDto.SortOrder,
                    Header = viewColumnDto.Header,
                    InheritHeader = viewColumnDto.InheritHeader,
                    Visible = viewColumnDto.Visible,
                    Sortable = viewColumnDto.Sortable,
                    ExtraSettings = viewColumnDto.ExtraSettings,
                    CustomColumnType = viewColumnDto.CustomColumnType,
                    Footer = viewColumnDto.Footer,
                    DbDocColumnMetadata = columnMetadata
                };

                result.Columns.Add(sectionViewColumn);

                if (section.View.GridView.DefaultSortColumnId is not null &&
                    section.View.GridView.DefaultSortColumnId == viewColumn.QueryTableColumnId)
                {
                    result.DefaultSortColumn = columnMetadata.StaticData.GetQueryAlias();
                }
            }

            foreach (var filterControl in section.View.Filters.OrderBy(x => x.SortOrder))
            {
                var filterControlDto = _mapper.Map<FilterControlDTO>(filterControl);
                var queryFilterBinding = filterControlDto.QueryFilterBindings // To think about multiple
                    .FirstOrDefault(x => x.QueryFilter is not null);

                var sectionViewFilter = new SectionViewFilterDTO
                {
                    FilterControlId = filterControlDto.Id,
                    AutoSubmitInput = filterControlDto.AutoSubmitInput,
                    ExtraSettings = filterControlDto.ExtraSettings,
                    HintText = filterControlDto.HintText,
                    Name = filterControlDto.Name,
                    InputType = filterControlDto.InputType,
                    DataType = filterControlDto.DataType,
                    SortOrder = filterControlDto.SortOrder,
                    UserCanChangeOperator = filterControlDto.UserCanChangeOperator,
                    QueryFilterId = queryFilterBinding?.QueryFilterId,
                    DbDocColumnId = queryFilterBinding?.QueryFilter.QueryTableColumn.SourceColumnId,
                    QueryRuleCode = queryFilterBinding?.QueryFilter.QueryRule.Code
                };

                result.Filters.Add(sectionViewFilter);
            }

            // Set master-section's emitted events that it's responsible for
            if (section.QueryFilterBindings.Any(x => x.BindingType == QueryFilterBindingType.MasterDetailGrid))
            {
                result.MasterSectionEmittedEvents.Add(MasterSectionEmitEventType.RowSelected);
            }

            // Set client-section bindings to master-section
            var bindings = section.Query.QueryFilterSets
                .SelectMany(x => x.QueryFilters)
                .SelectMany(x => x.QueryFilterBindings)
                .Where(x => x.BindingType == QueryFilterBindingType.MasterDetailGrid);

            if (bindings.Any())
            {
                foreach (var binding in bindings)
                {
                    var columnStaticData = _dbSchemaManager.GetColumn(binding.MasterDetailQueryTableColumn?.SourceColumnId);

                    var masterSectionBinding = new MasterSectionBindingDTO
                    {
                        MasterSectionId = binding.MasterDetailSectionId,
                        EventType = MasterSectionEmitEventType.RowSelected,
                        FilterId = FilterActionProvider.QueryFilterBindingAsFilterId(binding.Id),
                        ColumnId = columnStaticData?.GetQueryAlias()
                    };

                    result.MasterSectionBindings.Add(masterSectionBinding);
                }
            }

            return result;
        }

        public async Task ToggleGridViewColumnsSortable(IEnumerable<GridViewColumn> gridViewColumns, bool value, CancellationToken ct = default)
        {
            foreach (var column in gridViewColumns)
            {
                column.Sortable = value;
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task ToggleGridViewColumnsVisible(IEnumerable<GridViewColumn> gridViewColumns, bool value, CancellationToken ct = default)
        {
            foreach (var column in gridViewColumns)
            {
                column.Visible = value;
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task<FilterControl> UpdateFilterControl(int filterControlId, FilterControlDTO dto, CancellationToken ct = default)
        {
            var filterControl = await _context.Set<FilterControl>().SingleOrDefaultAsync(x => x.Id == filterControlId, ct)
                ?? throw new ObjectNotExistsException("The filter control with specified ID doesn't exist.");

            dto.Id = filterControlId;
            dto.ViewId = filterControl.ViewId;

            if (dto.SortOrder < filterControl.SortOrder)
            {
                await _context.Set<FilterControl>()
                    .Where(x => x.SortOrder >= dto.SortOrder && x.SortOrder < filterControl.SortOrder)
                    .ForEachAsync(x => x.SortOrder++);
            }

            if (dto.SortOrder > filterControl.SortOrder)
            {
                await _context.Set<FilterControl>()
                    .Where(x => x.SortOrder > filterControl.SortOrder && x.SortOrder <= dto.SortOrder)
                    .ForEachAsync(x => x.SortOrder--, ct);
            }

            _mapper.Map(dto, filterControl);

            await _context.SaveChangesAsync(ct);

            return filterControl;
        }

        public async Task<GridView> UpdateGridView(int gridViewId, GridViewDTO dto, CancellationToken ct = default)
        {
            var gridView = await _context.Set<GridView>().SingleOrDefaultAsync(x => x.Id == gridViewId, ct)
                ?? throw new ObjectNotExistsException("The table view with specified ID doesn't exist.");

            dto.Id = gridView.Id;
            dto.ViewId = gridView.ViewId;

            _mapper.Map(dto, gridView);

            await _context.SaveChangesAsync(ct);

            return gridView;
        }

        public async Task<GridViewColumn> UpdateGridViewColumn(int gridViewColumnId, GridViewColumnDTO dto, CancellationToken ct = default)
        {
            var gridViewColumn = await _context.Set<GridViewColumn>().SingleOrDefaultAsync(x => x.Id == gridViewColumnId, ct)
                ?? throw new ObjectNotExistsException("The table view column with specified ID doesn't exist.");

            dto.Id = gridViewColumnId;
            dto.GridViewId = gridViewColumn.GridViewId;
            dto.QueryTableColumnId = gridViewColumn.QueryTableColumnId;

            if (dto.SortOrder < gridViewColumn.SortOrder)
            {
                await _context.Set<GridViewColumn>()
                    .Where(x => x.SortOrder >= dto.SortOrder && x.SortOrder < gridViewColumn.SortOrder)
                    .ForEachAsync(x => x.SortOrder++, ct);
            }

            if (dto.SortOrder > gridViewColumn.SortOrder)
            {
                await _context.Set<GridViewColumn>()
                    .Where(x => x.SortOrder > gridViewColumn.SortOrder && x.SortOrder <= dto.SortOrder)
                    .ForEachAsync(x => x.SortOrder--, ct);
            }

            _mapper.Map(dto, gridViewColumn);

            await _context.SaveChangesAsync(ct);

            return gridViewColumn;
        }

        private async Task<(GridViewColumn, IList<GridViewColumn>)> CreateGridViewColumn(int gridViewId, int queryTableColumnId, CancellationToken ct = default)
        {
            var gridView = await _context.Set<GridView>()
                .Include(x => x.ViewColumns)
                .SingleOrDefaultAsync(x => x.Id == gridViewId, ct)
                ?? throw new ObjectNotExistsException("The table view with specified ID doesn't exist.");

            var queryTableColumn = await _context.Set<QueryTableColumn>()
                .Include(x => x.QueryTable).ThenInclude(x => x.Query)
                .SingleOrDefaultAsync(x => x.Id == queryTableColumnId, ct)
                ?? throw new ObjectNotExistsException("The query table column with specified ID doesn't exist.");

            if (gridView.ViewColumns.Any(x => x.QueryTableColumnId == queryTableColumn.Id))
                throw new BusinessException("The query table column already has an associated table view column.");

            return await CreateGridViewColumn(gridView, queryTableColumn, true, ct);
        }

        /// <param name="insertInSortedOrder">if found view columns of the same table as added column,
        /// then insert into position to make the columns ordered.
        /// </param>
        private async Task<(GridViewColumn, IList<GridViewColumn>)> CreateGridViewColumn(
            GridView gridView,
            QueryTableColumn queryTableColumn,
            bool insertInSortedOrder,
            CancellationToken ct = default)
        {
            IList<GridViewColumn> affectedColumns = new List<GridViewColumn>();

            var columnTitle = string.IsNullOrEmpty(queryTableColumn.QueryTable.SourceCode) ?
                await _context.Set<ColumnMetadata>()
                    .Where(x => x.Table.FolderId == queryTableColumn.QueryTable.Query.DbDocFolderId &&
                        x.ColumnId == queryTableColumn.SourceColumnId)
                    .Select(x => x.Title)
                    .SingleOrDefaultAsync(ct)
                //TODO: hack for demo
                : queryTableColumn.SourceColumnId;

            var columnStaticData = string.IsNullOrEmpty(queryTableColumn.QueryTable.SourceCode) ?
                (_dbSchemaManager.GetColumn(queryTableColumn.SourceColumnId)
                    ?? throw new ConflictException("The DBDoc column static data is missed for the query table column.")
                )
                : new DbSchemaColumn
                {
                    ParentTableName = queryTableColumn.QueryTable.SourceTableId,
                    ColumnName = queryTableColumn.SourceColumnId
                };

            #region Calculating column sort order
            int addedColumnSortOrder = -1;
            if (insertInSortedOrder)
            {
                var tableSortedColumns = _context.Set<GridViewColumn>()
                    .Where(x => x.QueryTableColumn.QueryTableId == queryTableColumn.QueryTableId)
                    .OrderByDescending(x => x.QueryTableColumn.SourceColumnId)
                    .Select(x => new { x.QueryTableColumn.SourceColumnId, x.SortOrder })
                    .ToList();

                if (tableSortedColumns.Any())
                {
                    var prevColumn = tableSortedColumns.
                        FirstOrDefault(x => x.SourceColumnId.CompareTo(queryTableColumn.SourceColumnId) == -1);
                    addedColumnSortOrder = prevColumn is null ?
                        tableSortedColumns.Last().SortOrder : prevColumn.SortOrder + 1;
                }
            }

            if (addedColumnSortOrder == -1)
            {
                addedColumnSortOrder = gridView.ViewColumns.Any() ? gridView.ViewColumns.Max(x => x.SortOrder) + 1 : 1;
            }
            else
            {
                // If found a column with the same sort order value, it means it should become the next column in order
                // after the new one and all the below columns should be shifted +1
                if (gridView.ViewColumns.Any(x => x.SortOrder == addedColumnSortOrder))
                {
                    foreach (var item in gridView.ViewColumns.Where(x => x.SortOrder >= addedColumnSortOrder))
                    {
                        item.SortOrder++;
                        affectedColumns.Add(item);
                    }
                }

            }
            #endregion

            var gridViewColumn = new GridViewColumn
            {
                QueryTableColumnId = queryTableColumn.Id,
                SortOrder = addedColumnSortOrder,
                ExtraSettings = "{}",
                Sortable = true,
                Visible = !columnStaticData.IsPrimaryKey.GetValueOrDefault()
                           && !columnStaticData.IsForeignKey.GetValueOrDefault(),
                GridViewId = gridView.Id,
                Header = columnTitle ?? $"{columnStaticData.ParentTableName}.{columnStaticData.ColumnName}"
            };
            await _context.Set<GridViewColumn>().AddAsync(gridViewColumn, ct);

            return (gridViewColumn, affectedColumns);
        }

        private static IEnumerable<InputType> GetInputTypesSuitableForClrType(ClrTypeGroup clrType) =>
            clrType switch
            {
                ClrTypeGroup.Date => new[] { InputType.Calendar, InputType.Dropdown, InputType.Text },
                ClrTypeGroup.Bool => new[] { InputType.Checkbox },
                ClrTypeGroup.Numeric => new[] { InputType.Number, InputType.Dropdown, InputType.Text },
                ClrTypeGroup.String => new[] { InputType.Dropdown, InputType.Text },
                _ => new[] { InputType.Calendar, InputType.Checkbox, InputType.Number, InputType.Dropdown, InputType.Text }
            };
    }
}
