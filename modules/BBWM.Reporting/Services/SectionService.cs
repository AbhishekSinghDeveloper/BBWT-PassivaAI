using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;
using SqlKata.Execution;

namespace BBWM.Reporting.Services;

public class SectionService : DataService, ISectionService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly IViewBuilderService _viewBuilderService;
    private readonly IQueryDataService _queryDataService;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IDbSchemaManager _dbSchemaManager;

    public SectionService(
        IDbContext context,
        IMapper mapper,
        IQueryBuilderService queryBuilderService,
        IViewBuilderService viewBuilderService,
        IQueryDataService queryDataService,
        IDbDocFolderService dbDocFolderService,
        IDbSchemaManager dbSchemaManager) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _queryBuilderService = queryBuilderService;
        _viewBuilderService = viewBuilderService;
        _queryDataService = queryDataService;
        _dbDocFolderService = dbDocFolderService;
        _dbSchemaManager = dbSchemaManager;
    }


    public async Task<ReportChangeResult> AddDuplicateQueryTable(Guid sectionId, QueryTableJoinDTO join, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>().Include(x => x.Query).SingleOrDefaultAsync(x => x.Id == sectionId, ct);
        var queryTable = await _context.Set<QueryTable>().SingleAsync(x => x.Id == join.FromQueryTableId, ct);
        var tableMetadata = await _context.Set<TableMetadata>().SingleAsync(x => x.TableId == queryTable.SourceTableId && x.FolderId == section.Query.DbDocFolderId, ct);

        section = await CheckSectionQuery(sectionId, tableMetadata.FolderId, ct);

        section.Report.UpdatedOn = DateTime.UtcNow;

        var query = await _context.Set<Query>()
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTableColumn)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTableColumn)
            .Include(x => x.QueryTables).ThenInclude(x => x.Columns)
            .SingleOrDefaultAsync(x => x.Id == section.QueryId, ct)
            ?? throw new ObjectNotExistsException($"The query with specified ID doesn't exist.");

        var duplicateJoin = await _queryBuilderService.AddDuplicateQueryTable(query, join, ct);
        var reportChangeResult = new ReportChangeResult
        {
            ReportUpdatedOn = section.Report.UpdatedOn,
            RequestTargetPart = _mapper.Map<QueryTableJoinDTO>(duplicateJoin)
        };

        var gridViewColumns = (await _viewBuilderService.AddGridViewColumns(section.View.GridView.Id, (int)duplicateJoin.FromQueryTableId, ct))
            .Concat(await _viewBuilderService.AddGridViewColumns(section.View.GridView.Id, (int)duplicateJoin.ToQueryTableId, ct));
        var gridViewColumnsDto = _mapper.Map<IEnumerable<GridViewColumnDTO>>(gridViewColumns);

        foreach (var gridViewColumnDto in gridViewColumnsDto)
        {
            reportChangeResult.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = gridViewColumnDto,
                ChangedPartName = ReportAdditionalChangedPart.GridViewColumnAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Created
            });
        }

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> AddFilterControl(
        Guid sectionId,
        FilterControlDTO dto,
        CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Report)
            .Include(x => x.View)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        section.Report.UpdatedOn = DateTime.UtcNow;

        var addedFilterControl = await _viewBuilderService.AddFilterControl(section.View.Id, dto, ct);

        var result = new ReportChangeResult
        {
            RequestTargetPart = _mapper.Map<FilterControlDTO>(addedFilterControl),
            ReportUpdatedOn = section.Report.UpdatedOn
        };

        var addedBindings = await _viewBuilderService.BindFilterControlToNewQueryFilters(
            dto.QueryFilterBindings.Where(x => x.Id == default),
            addedFilterControl,
            ct);

        foreach (var binding in addedBindings)
        {
            result.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = binding,
                ChangedPartType = ReportChangeType.Created,
                ChangedPartName = ReportAdditionalChangedPart.QueryFilterBindingAdditionalChangedPartName
            });
        }

        return result;
    }

    public async Task<ReportChangeResult> AddQueryFilter(Guid sectionId, QueryFilterDTO dto, CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        var result = _mapper.Map<QueryFilterDTO>(await _queryBuilderService.AddQueryFilter(dto, ct));
        reportChangeResult.RequestTargetPart = result;

        if (result.QueryFilterBindings.Any())
        {
            var firstBinding = result.QueryFilterBindings.First();
            reportChangeResult.AdditionalChangedParts = new List<ReportAdditionalChangedPart>
            {
                new ReportAdditionalChangedPart
                {
                    ChangedPartType = ReportChangeType.Created,
                    ChangedPartName = ReportAdditionalChangedPart.QueryFilterBindingAdditionalChangedPartName,
                    ChangedPartData = firstBinding,
                    ChangedPartId = firstBinding.Id
                }
            };
            result.QueryFilterBindings.ToList().Clear();
        }

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> AddQueryFilterSet(
        Guid sectionId,
        int parentQueryFilterSetId,
        CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        reportChangeResult.RequestTargetPart = _mapper.Map<QueryFilterSetDTO>(
            await _queryBuilderService.AddQueryFilterSet(parentQueryFilterSetId, ct));

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> AddQueryTable(Guid sectionId, int tableMetadataId, CancellationToken ct = default)
    {
        var tableMetadata = await _context.Set<TableMetadata>().SingleOrDefaultAsync(x => x.Id == tableMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The table '{tableMetadataId}' doesn't exist.");

        var section = await CheckSectionQuery(sectionId, tableMetadata.FolderId, ct);

        section.Report.UpdatedOn = DateTime.UtcNow;

        var query = await _context.Set<Query>()
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTableColumn)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable)
            .Include(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTableColumn)
            .Include(x => x.QueryTables).ThenInclude(x => x.Columns)
            .SingleOrDefaultAsync(x => x.Id == section.QueryId, ct)
            ?? throw new ObjectNotExistsException($"The query with specified ID doesn't exist.");

        var queryTableCreationResult = await _queryBuilderService.AddQueryTable(query, tableMetadataId, ct);

        var reportChangeResult = new ReportChangeResult
        {
            ReportUpdatedOn = section.Report.UpdatedOn,
            RequestTargetPart = _mapper.Map<QueryTableDTO>(queryTableCreationResult.queryTable)
        };

        foreach (var queryTableJoin in queryTableCreationResult.joins)
        {
            reportChangeResult.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = Mapper.Map<QueryTableJoinDTO>(queryTableJoin),
                ChangedPartName = ReportAdditionalChangedPart.QueryTableJoinAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Created
            });
        }

        var gridViewColumns = await _viewBuilderService.AddGridViewColumns(section.View.GridView.Id, queryTableCreationResult.queryTable.Id, ct);
        var gridViewColumnsDto = _mapper.Map<IEnumerable<GridViewColumnDTO>>(gridViewColumns);

        foreach (var gridViewColumnDto in gridViewColumnsDto)
        {
            reportChangeResult.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = gridViewColumnDto,
                ChangedPartName = ReportAdditionalChangedPart.GridViewColumnAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Created
            });
        }

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> AddQueryTablesFromSource(Guid sectionId, QueryableTableSource[] sources, CancellationToken ct)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Report)
            .Include(x => x.View).ThenInclude(x => x.GridView)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        section.Report.UpdatedOn = DateTime.UtcNow;

        var query = await _context.Set<Query>().SingleOrDefaultAsync(x => x.Id == section.QueryId, ct)
            ?? throw new ObjectNotExistsException($"The query with specified ID doesn't exist.");

        var queryTables = await _queryBuilderService.AddQueryTablesFromSource(query, sources, ct);

        var reportChangeResult = new ReportChangeResult
        {
            ReportUpdatedOn = section.Report.UpdatedOn,
            RequestTargetPart = _mapper.Map<List<QueryTableDTO>>(queryTables)
        };


        foreach (var quertTable in queryTables)
        {
            var gridViewColumns = await _viewBuilderService.AddGridViewColumns(section.View.GridView.Id, quertTable.Id, ct);

            var gridViewColumnsDto = _mapper.Map<IEnumerable<GridViewColumnDTO>>(gridViewColumns);

            foreach (var gridViewColumnDto in gridViewColumnsDto)
            {
                reportChangeResult.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
                {
                    ChangedPartData = gridViewColumnDto,
                    ChangedPartName = ReportAdditionalChangedPart.GridViewColumnAdditionalChangedPartName,
                    ChangedPartType = ReportChangeType.Created
                });
            }
        }

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> AddQueryTableColumn(Guid sectionId, int columnMetadataId, int? parentQueryTableId, CancellationToken ct = default)
    {
        var columnMetadata = await _context.Set<ColumnMetadata>()
            .Include(x => x.Table)
            .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column '{columnMetadataId}' doesn't exist.");

        var section = await CheckSectionQuery(sectionId, columnMetadata.Table.FolderId, ct);

        section.Report.UpdatedOn = DateTime.UtcNow;

        var query = await _context.Set<Query>()
            .Include(x => x.QueryTables).ThenInclude(x => x.Columns)
            .SingleOrDefaultAsync(x => x.Id == section.QueryId, ct)
            ?? throw new ObjectNotExistsException($"The query with specified ID doesn't exist.");

        var queryTableColumnCreationResult = await _queryBuilderService.AddQueryTableColumn(query, columnMetadataId, parentQueryTableId, ct);

        var addedGridViewColumnResult = await _viewBuilderService.AddGridViewColumn(
            section.View.GridView.Id, queryTableColumnCreationResult.queryTableColumn.Id, query, ct);

        var additionalChangedParts = new List<ReportAdditionalChangedPart>
        {
            new ReportAdditionalChangedPart
            {
                ChangedPartData = _mapper.Map<GridViewColumnDTO>(addedGridViewColumnResult.Item1),
                ChangedPartName = ReportAdditionalChangedPart.GridViewColumnAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Created
            }
        };

        var affectedGridViewColumnsDto = _mapper.Map<IEnumerable<GridViewColumnDTO>>(addedGridViewColumnResult.Item2);
        foreach (var affectedColumnDto in affectedGridViewColumnsDto)
        {
            additionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = affectedColumnDto,
                ChangedPartName = ReportAdditionalChangedPart.GridViewColumnAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Modified
            });
        }

        foreach (var queryTableJoin in queryTableColumnCreationResult.joins)
        {
            additionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = Mapper.Map<QueryTableJoinDTO>(queryTableJoin),
                ChangedPartName = ReportAdditionalChangedPart.QueryTableJoinAdditionalChangedPartName,
                ChangedPartType = ReportChangeType.Created
            });
        }

        return new ReportChangeResult
        {
            ReportUpdatedOn = section.Report.UpdatedOn,
            RequestTargetPart = _mapper.Map<QueryTableColumnDTO>(queryTableColumnCreationResult.queryTableColumn),
            AdditionalChangedParts = additionalChangedParts
        };
    }

    public async Task<ReportChangeResult> AddQueryTableJoin(Guid sectionId, QueryTableJoinDTO joinDto, CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        reportChangeResult.RequestTargetPart = _mapper.Map<QueryTableJoinDTO>(
            await _queryBuilderService.AddQueryTableJoin(joinDto, ct));

        return reportChangeResult;
    }

    public async Task<ReportChangeResult> BindFilterControlToQueryFilter(
        Guid sectionId,
        int filterControlId,
        int queryFilterId,
        CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Report)
            .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters)
            .Include(x => x.View).ThenInclude(x => x.Filters)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        if (section.Query.QueryFilterSets.SelectMany(x => x.QueryFilters).All(x => x.Id != queryFilterId))
            throw new ObjectNotExistsException("The query filter with specified ID doesn't exist in the section.");

        if (section.View.Filters.All(x => x.Id != filterControlId))
            throw new ObjectNotExistsException("The filter control with specified ID doesn't exist in the section.");

        section.Report.UpdatedOn = DateTime.UtcNow;

        return new ReportChangeResult
        {
            ReportUpdatedOn = section.Report.UpdatedOn,
            RequestTargetPart = _mapper.Map<QueryFilterBindingDTO>(
                await _viewBuilderService.BindFilterControlToQueryFilter(filterControlId, queryFilterId, ct))
        };
    }

    public Task<ReportChangeResult> DeleteQueryTableColumn(Guid sectionId, int queryTableColumnId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _queryBuilderService.DeleteQueryTableColumn, queryTableColumnId, ct);

    public async Task<ReportChangeResult> DeleteFilterControl(
        Guid sectionId,
        int filterControlId,
        bool deleteLinkedQueryFilters,
        CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };
        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);
        await _viewBuilderService.DeleteFilterControl(filterControlId, deleteLinkedQueryFilters, ct);

        return reportChangeResult;
    }

    public Task<ReportChangeResult> DeleteQueryFilter(Guid sectionId, int queryFilterId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _queryBuilderService.DeleteQueryFilter, queryFilterId, ct);

    public Task<ReportChangeResult> DeleteQueryFilterBinding(Guid sectionId, int filterControlBindingId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _viewBuilderService.DeleteQueryFilterBinding, filterControlBindingId, ct);

    public Task<ReportChangeResult> DeleteQueryFilterSet(Guid sectionId, int queryFilterSetId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _queryBuilderService.DeleteQueryFilterSet, queryFilterSetId, ct);

    public Task<ReportChangeResult> DeleteQueryTable(Guid sectionId, int queryTableId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _queryBuilderService.DeleteQueryTable, queryTableId, ct);

    public Task<ReportChangeResult> DeleteQueryTableJoin(Guid sectionId, int queryTableJoinId, CancellationToken ct = default)
        => DeleteSectionPart(sectionId, _queryBuilderService.DeleteQueryTableJoin, queryTableJoinId, ct);

    public Task<bool> Exists(Guid sectionId, CancellationToken ct = default)
        => _context.Set<Section>().AnyAsync(x => x.Id == sectionId, ct);

    public async Task<IEnumerable<string>> GetReachableTables(Guid sectionId, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Query).ThenInclude(x => x.QueryTables)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        if (section.Query.QueryTables.Count == 0)
            return null;

        var firstTable = section.Query.QueryTables.First();

        // TODO: for now we don't handle table references in query tables of external sources (forms),
        // therefore no need to get reachable tables.
        if (!string.IsNullOrEmpty(firstTable.SourceCode))
            return new List<string>();

        var result = _queryBuilderService.GetReachableTables(firstTable.SourceTableId).ToList();
        result.Insert(0, firstTable.SourceTableId);
        return result;
    }

    public async Task<IEnumerable<TableMetadataDTO>> GetSectionTablesMatadata(Guid sectionId, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Query).ThenInclude(x => x.QueryTables)
            .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets)
                .ThenInclude(x => x.QueryFilters).ThenInclude(x => x.QueryFilterBindings)
                .ThenInclude(x => x.MasterDetailQueryTableColumn).ThenInclude(x => x.QueryTable)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        if (section.Query.DbDocFolderId is null)
            return new List<TableMetadataDTO>();

        return await _dbDocFolderService.GetFullTablesMatadata(
            (Guid)section.Query.DbDocFolderId,
            section.Query.QueryTables.Select(x => x.SourceTableId).Union(
                section.Query.QueryFilterSets
                    .SelectMany(x => x.QueryFilters)
                    .SelectMany(x => x.QueryFilterBindings)
                    .Where(x => x.BindingType == QueryFilterBindingType.MasterDetailGrid)
                    .Select(x => x.MasterDetailQueryTableColumn.QueryTable.SourceTableId)),
            ct);
    }

    public async Task<QueryDTO> GetQueryStructure(Guid sectionId, CancellationToken ct = default)
        => _mapper.Map<QueryDTO>((await _context.Set<Section>()
            .Include(x => x.Query)
                .ThenInclude(x => x.QueryTables).ThenInclude(x => x.Columns)
            .Include(x => x.Query)
                .ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable)
            .Include(x => x.Query)
                .ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable)
            .Include(x => x.Query)
                .ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters)
                .ThenInclude(x => x.QueryFilterBindings).ThenInclude(x => x.MasterDetailQueryTableColumn)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID does not exist."))?.Query
            ?? throw new ConflictException("The section is not associated with any Query object."));

    public async Task<string> GetSqlQuery(Guid sectionId, bool reduceSyntax = false, CancellationToken ct = default)
    {
        var query = _context.Set<Section>()
            .Include(x => x.Query).ThenInclude(x => x.QueryTables).ThenInclude(x => x.Columns)
            .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets).ThenInclude(x => x.QueryFilters).ThenInclude(x => x.QueryRule)
            .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable).ThenInclude(x => x.Columns)
            .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable).ThenInclude(x => x.Columns)
            .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTableColumn).ThenInclude(x => x.QueryTable)
            .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTableColumn).ThenInclude(x => x.QueryTable);

        var fullSection = await query.AsNoTracking().SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID does not exist.");

        if (fullSection.Query is null) return null;

        fullSection.Query.RootFilterSet = fullSection.Query.QueryFilterSets.Any()
            ? QueryBuilderService.MakeFilterSetsTree(fullSection.Query.QueryFilterSets, false)
            : null;

        var sqlQuery = await _queryDataService.GetSqlQuery(fullSection.Query, reduceSyntax, ct);

        return sqlQuery;
    }

    public async Task<ViewDTO> GetView(Guid sectionId, CancellationToken ct = default)
        => _mapper.Map<ViewDTO>((await _context.Set<Section>()
            .Include(x => x.View).ThenInclude(x => x.Filters)
                .ThenInclude(x => x.QueryFilterBindings)
            .Include(x => x.View)
                .ThenInclude(x => x.GridView).ThenInclude(x => x.ViewColumns)
            .AsSplitQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID does not exist."))?.View
            ?? throw new ConflictException("The section is not associated with any View object."));

    public async Task ToggleSectionGridViewColumnsSortable(Guid sectionId, bool value, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.View).ThenInclude(x => x.GridView).ThenInclude(x => x.ViewColumns)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct);

        if (section is null)
            throw new ObjectNotExistsException("The section with specified ID does not exist.");

        await _viewBuilderService.ToggleGridViewColumnsSortable(section.View.GridView.ViewColumns, value, ct);
    }

    public async Task ToggleSectionGridViewColumnsVisible(Guid sectionId, bool value, CancellationToken ct = default)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Query).ThenInclude(x => x.QueryTables).ThenInclude(x => x.Columns)
            .Include(x => x.View).ThenInclude(x => x.GridView).ThenInclude(x => x.ViewColumns)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct);

        var sectionColumnIdToDbDocColumnIdMap = section.Query.QueryTables.SelectMany(x => x.Columns).ToDictionary(x => x.Id, x => x.SourceColumnId);

        if (section is null)
            throw new ObjectNotExistsException("The section with specified ID does not exist.");

        await _viewBuilderService.ToggleGridViewColumnsVisible(
            section.View.GridView.ViewColumns.Where(x => !(_dbSchemaManager.GetColumn(sectionColumnIdToDbDocColumnIdMap[x.QueryTableColumnId])?.IsForeignKey ?? false) &&
                !(_dbSchemaManager.GetColumn(sectionColumnIdToDbDocColumnIdMap[x.QueryTableColumnId]).IsPrimaryKey ?? false)),
            value,
            ct);
    }

    public async Task<ReportChangeResult> UpdateFilterControl(
        Guid sectionId,
        int filterControlId,
        FilterControlDTO dto,
        CancellationToken ct = default)
    {
        var result = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, result, ct);

        var filterControl = await _viewBuilderService.UpdateFilterControl(filterControlId, dto, ct);

        result.RequestTargetPart = _mapper.Map<FilterControlDTO>(filterControl);

        var addedBindings = await _viewBuilderService.BindFilterControlToNewQueryFilters(
            dto.QueryFilterBindings.Where(x => x.Id == default),
            filterControl,
            ct);

        foreach (var binding in addedBindings)
        {
            result.AdditionalChangedParts.Add(new ReportAdditionalChangedPart
            {
                ChangedPartData = binding,
                ChangedPartType = ReportChangeType.Created,
                ChangedPartName = ReportAdditionalChangedPart.QueryFilterBindingAdditionalChangedPartName
            });
        }

        return result;
    }

    public async Task<ReportChangeResult> UpdateMasterDetailQueryFilterBinding(Guid sectionId, QueryFilterBindingDTO dto, CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        reportChangeResult.RequestTargetPart = _mapper.Map<QueryFilterBindingDTO>(
            await _queryBuilderService.UpdateMasterDetailQueryFilterBinding(dto.Id, dto, ct));
        reportChangeResult.AdditionalChangedParts = new List<ReportAdditionalChangedPart>()
        {
            new ReportAdditionalChangedPart
            {
                ChangedPartName = ReportAdditionalChangedPart.QueryFilterAdditionalChangedPartName,
                ChangedPartData = _mapper.Map<QueryFilterDTO>(
                    await _queryBuilderService.UpdateQueryFilter(dto.QueryFilterId, dto.QueryFilter, ct)),
                ChangedPartId = dto.QueryFilterId,
                ChangedPartType = ReportChangeType.Modified,
            }
        };

        return reportChangeResult;
    }

    public Task<ReportChangeResult> UpdateQueryFilter(
        Guid sectionId,
        int queryFilterId,
        QueryFilterDTO dto,
        CancellationToken ct = default)
        => UpdateSectionPart(sectionId, _queryBuilderService.UpdateQueryFilter, queryFilterId, dto, ct);

    public Task<ReportChangeResult> UpdateSqlFilter(
        Guid sectionId,
        int queryFilterId,
        QueryFilterDTO dto,
        CancellationToken ct = default)
        => UpdateSectionPart(sectionId, _queryBuilderService.UpdateSqlFilter, queryFilterId, dto, ct);

    public Task<ReportChangeResult> UpdateQueryFilterSet(
        Guid sectionId,
        int queryFilterSetId,
        QueryFilterSetDTO dto,
        CancellationToken ct = default)
        => UpdateSectionPart(sectionId, _queryBuilderService.UpdateQueryFilterSet, queryFilterSetId, dto, ct);

    public async Task<ReportChangeResult> UpdateQueryTableJoin(Guid sectionId, QueryTableJoinDTO joinDto, CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        reportChangeResult.RequestTargetPart = _mapper.Map<QueryTableJoinDTO>(
            await _queryBuilderService.UpdateQueryTableJoin(joinDto, ct));

        return reportChangeResult;
    }

    public Task<ReportChangeResult> UpdateGridView(Guid sectionId, int gridViewId, GridViewDTO dto, CancellationToken ct = default)
        => UpdateSectionPart(sectionId, _viewBuilderService.UpdateGridView, gridViewId, dto, ct);

    public Task<ReportChangeResult> UpdateGridViewColumn(
        Guid sectionId,
        int gridViewColumnId,
        GridViewColumnDTO dto,
        CancellationToken ct = default)
        => UpdateSectionPart(sectionId, _viewBuilderService.UpdateGridViewColumn, gridViewColumnId, dto, ct);

    private async Task<Section> CheckSectionQuery(Guid sectionId, Guid folderId, CancellationToken ct)
    {
        var section = await _context.Set<Section>()
            .Include(x => x.Report)
            .Include(x => x.View).ThenInclude(x => x.GridView)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        if (section.QueryId is null)
        {
            var newQuery = await _queryBuilderService.CreateQuery(folderId, ct);

            await _context.Set<Query>().AddAsync(newQuery, ct);
            await _context.SaveChangesAsync(ct);

            section.QueryId = newQuery.Id;
        }

        return section;
    }

    private async Task<ReportChangeResult> DeleteSectionPart(
        Guid sectionId,
        Func<int, CancellationToken, Task> deleteFunc,
        int partId,
        CancellationToken ct)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);
        await deleteFunc(partId, ct);

        return reportChangeResult;
    }

    private async Task RefreshSectionsReportUpdatedOn(Guid sectionId, ReportChangeResult reportChangeResult, CancellationToken ct)
    {
        var section = await _context.Set<Section>().Include(x => x.Report)
            .SingleOrDefaultAsync(x => x.Id == sectionId, ct)
            ?? throw new ObjectNotExistsException("The section with specified ID doesn't exist.");

        section.Report.UpdatedOn = reportChangeResult.ReportUpdatedOn;
    }

    private async Task<ReportChangeResult> UpdateSectionPart<TEntity, TDTO>(
        Guid sectionId,
        Func<int, TDTO, CancellationToken, Task<TEntity>> updateFunc,
        int partId,
        TDTO dto,
        CancellationToken ct = default)
    {
        var reportChangeResult = new ReportChangeResult { ReportUpdatedOn = DateTime.UtcNow };

        await RefreshSectionsReportUpdatedOn(sectionId, reportChangeResult, ct);

        reportChangeResult.RequestTargetPart = _mapper.Map<TDTO>(await updateFunc(partId, dto, ct));

        return reportChangeResult;
    }
}