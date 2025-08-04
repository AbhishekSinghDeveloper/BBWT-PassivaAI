using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.Core.Web.ModelBinders;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Reporting.Controllers
{
    [Route("api/reporting/section")]
    [ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SuperAdminRole + "," + Roles.ReportEditorRole)]
    public class SectionController : DataControllerBase<Section, SectionDTO, SectionDTO, Guid>
    {
        private readonly ISectionService _sectionService;
        private readonly IQueryableTableSourceService _querableTableSourceService;

        public SectionController(
            IDataService dataService,
            ISectionService sectionService,
            IQueryableTableSourceService querableTableSourceService) : base(dataService)
        {
            _sectionService = sectionService;
            _querableTableSourceService = querableTableSourceService;
        }

        #region Section CRUD
        public override async Task<IActionResult> Create(
            [FromBody] SectionDTO dto,
            [FromServices] IModelHashingService modelHashingService,
            CancellationToken ct = default) => NotFound();

        public override async Task<IActionResult> Update([FromBody] SectionDTO dto, CancellationToken ct = default) => NotFound();

        public override async Task<IActionResult> Delete([HashedKeyBinder] Guid id, CancellationToken ct = default) => NotFound();
        #endregion

        #region Query Builder
        [HttpPost("{sectionId}/add-duplicate-query-table")]
        public async Task<IActionResult> AddDuplicateQueryTable(Guid sectionId, [FromBody] QueryTableJoinDTO join, CancellationToken ct)
            => Ok(await _sectionService.AddDuplicateQueryTable(sectionId, join, ct));

        [HttpPost("{sectionId}/add-filter-control")]
        public async Task<IActionResult> AddFilterControl(
            Guid sectionId,
            [FromBody] FilterControlDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.AddFilterControl(sectionId, dto, ct));

        [HttpPost("{sectionId}/add-query-filter")]
        public async Task<IActionResult> AddQueryFilter(Guid sectionId, [FromBody] QueryFilterDTO dto, CancellationToken ct)
            => Ok(await _sectionService.AddQueryFilter(sectionId, dto, ct));

        [HttpPost("{sectionId}/add-query-filter-set/{parentQueryFilterSetId}")]
        public async Task<IActionResult> AddQueryFilterSet(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterSetDTO))] int parentQueryFilterSetId,
            CancellationToken ct)
            => Ok(await _sectionService.AddQueryFilterSet(sectionId, parentQueryFilterSetId, ct));

        [HttpPost("{sectionId}/add-query-table/{tableMetadataId}")]
        public async Task<IActionResult> AddQueryTable(Guid sectionId, int tableMetadataId, CancellationToken ct)
            => Ok(await _sectionService.AddQueryTable(sectionId, tableMetadataId, ct));

        [HttpPost("{sectionId}/add-query-tables-from-source")]
        public async Task<IActionResult> AddQueryTablesFromSource(Guid sectionId,
            [FromBody] QueryableTableSource[] sources, CancellationToken ct)
            => Ok(await _sectionService.AddQueryTablesFromSource(sectionId, sources, ct));

        [HttpPost("{sectionId}/add-query-table-column/{columnMetadataId}")]
        public async Task<IActionResult> AddQueryTableColumn(
            Guid sectionId,
            int columnMetadataId,
            [FromBody] int? parentQueryTableId,
            CancellationToken ct)
            => Ok(await _sectionService.AddQueryTableColumn(sectionId, columnMetadataId, parentQueryTableId, ct));

        [HttpPost("{sectionId}/add-query-table-join")]
        public async Task<IActionResult> AddQueryTableJoin(
            Guid sectionId,
            [FromBody] QueryTableJoinDTO queryTableJoinDto,
            CancellationToken ct)
            => Ok(await _sectionService.AddQueryTableJoin(sectionId, queryTableJoinDto, ct));

        /// <summary>
        /// This method version supposes that if query filter is already bounded to a filter control then it
        /// automatically re-bind it to a new filter control by passing allowRebind = true
        /// </summary>
        /// <returns>Section object</returns>
        [HttpPost("{sectionId}/bind-filter-control/{filterControlId}/to-query-filter/{queryFilterId}")]
        public async Task<IActionResult> BindFilterControlToQueryFilter(
            Guid sectionId,
            [HashedKeyBinder(typeof(FilterControlDTO))] int filterControlId,
            [HashedKeyBinder(typeof(QueryFilterDTO))] int queryFilterId,
            CancellationToken ct)
            => Ok(await _sectionService.BindFilterControlToQueryFilter(sectionId, filterControlId, queryFilterId, ct));

        [HttpDelete("{sectionId}/delete-filter-control/{filterControlId}")]
        public async Task<IActionResult> DeleteFilterControl(
            Guid sectionId,
            [HashedKeyBinder(typeof(FilterControlDTO))] int filterControlId,
            [FromQuery(Name = "deleteLinkedQueryFilters")] bool deleteLinkedQueryFilters,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteFilterControl(sectionId, filterControlId, deleteLinkedQueryFilters, ct));

        [HttpDelete("{sectionId}/delete-query-filter/{queryFilterId}")]
        public async Task<IActionResult> DeleteQueryFilter(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterDTO))] int queryFilterId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryFilter(sectionId, queryFilterId, ct));

        [HttpDelete("{sectionId}/delete-query-filter-binding/{queryFilterBindingId}")]
        public async Task<IActionResult> DeleteQueryFilterBinding(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterBindingDTO))] int queryFilterBindingId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryFilterBinding(sectionId, queryFilterBindingId, ct));

        [HttpDelete("{sectionId}/delete-query-filter-set/{queryFilterSetId}")]
        public async Task<IActionResult> DeleteQueryFilterSet(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterSetDTO))] int queryFilterSetId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryFilterSet(sectionId, queryFilterSetId, ct));

        [HttpDelete("{sectionId}/delete-query-table/{queryTableId}")]
        public async Task<IActionResult> DeleteQueryTable(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryTableDTO))] int queryTableId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryTable(sectionId, queryTableId, ct));

        [HttpDelete("{sectionId}/delete-query-table-column/{queryTableColumnId}")]
        public async Task<IActionResult> DeleteQueryTableColumn(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryTableColumnDTO))] int queryTableColumnId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryTableColumn(sectionId, queryTableColumnId, ct));

        [HttpDelete("{sectionId}/delete-query-table-join/{queryTableJoinId}")]
        public async Task<IActionResult> DeleteQueryTableJoin(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryTableJoinDTO))] int queryTableJoinId,
            CancellationToken ct)
            => Ok(await _sectionService.DeleteQueryTableJoin(sectionId, queryTableJoinId, ct));

        [HttpGet("quaryable-table-sources")]
        public async Task<IActionResult> GetQueryableTableSources(CancellationToken ct)
            => Ok(await _querableTableSourceService.GetQueryableTableSources(ct));

        [HttpGet("{sectionId}/tables-metadata")]
        public async Task<IActionResult> GetTablesMetadata(Guid sectionId, CancellationToken ct)
            => Ok(await _sectionService.GetSectionTablesMatadata(sectionId, ct));

        [HttpGet("{sectionId}/query-structure")]
        public async Task<IActionResult> GetQueryStructure(Guid sectionId, CancellationToken ct)
            => Ok(await _sectionService.GetQueryStructure(sectionId, ct));

        [HttpGet("{sectionId}/reachable-tables")]
        public async Task<IActionResult> GetReachableTables(Guid sectionId, CancellationToken ct)
            => Ok(await _sectionService.GetReachableTables(sectionId, ct));

        [HttpGet("{sectionId}/sql-query")]
        public async Task<IActionResult> GetSqlQuery(Guid sectionId, CancellationToken ct)
            => Ok(new { Sql = await _sectionService.GetSqlQuery(sectionId, true, ct) });

        [HttpPost("{sectionId}/toggle-grid-view-columns-sortable")]
        public Task<IActionResult> ToggleSectionGridViewColumnsSortable(Guid sectionId, [FromBody] bool value, CancellationToken ct)
            => NoContent(() => _sectionService.ToggleSectionGridViewColumnsSortable(sectionId, value, ct));

        [HttpPost("{sectionId}/toggle-grid-view-columns-visible")]
        public Task<IActionResult> ToggleSectionGridViewColumnsVisible(Guid sectionId, [FromBody] bool value, CancellationToken ct)
            => NoContent(() => _sectionService.ToggleSectionGridViewColumnsVisible(sectionId, value, ct));

        [HttpPut("{sectionId}/update-filter-control/{filterControlId}")]
        public async Task<IActionResult> UpdateFilterControl(
            Guid sectionId,
            [HashedKeyBinder(typeof(FilterControlDTO))] int filterControlId,
            [FromBody] FilterControlDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateFilterControl(sectionId, filterControlId, dto, ct));

        [HttpPost("{sectionId}/update-master-detail-query-filter-binding")]
        public async Task<IActionResult> UpdateMasterDetailQueryFilterBinding(Guid sectionId, [FromBody] QueryFilterBindingDTO dto, CancellationToken ct)
            => Ok(await _sectionService.UpdateMasterDetailQueryFilterBinding(sectionId, dto, ct));

        [HttpPut("{sectionId}/update-query-filter/{queryFilterId}")]
        public async Task<IActionResult> UpdateQueryFilter(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterDTO))] int queryFilterId,
            [FromBody] QueryFilterDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateQueryFilter(sectionId, queryFilterId, dto, ct));

        [HttpPut("{sectionId}/update-sql-filter/{queryFilterId}")]
        public async Task<IActionResult> UpdateSqlFilter(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterDTO))] int queryFilterId,
            [FromBody] QueryFilterDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateSqlFilter(sectionId, queryFilterId, dto, ct));

        [HttpPut("{sectionId}/update-query-filter-set/{queryFilterSetId}")]
        public async Task<IActionResult> UpdateQueryFilterSet(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryFilterSetDTO))] int queryFilterSetId,
            [FromBody] QueryFilterSetDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateQueryFilterSet(sectionId, queryFilterSetId, dto, ct));

        [HttpPost("{sectionId}/update-query-table-join/{queryTableJoinId}")]
        public async Task<IActionResult> UpdateQueryFilterSet(
            Guid sectionId,
            [HashedKeyBinder(typeof(QueryTableJoinDTO))] int queryTableJoinId,
            [FromBody] QueryTableJoinDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateQueryTableJoin(sectionId, dto, ct));
        #endregion

        #region Grid View Settings
        [HttpGet("{sectionId}/view-settings")]
        public async Task<IActionResult> GetViewSettings(Guid sectionId, CancellationToken ct)
            => Ok(await _sectionService.GetView(sectionId, ct));

        [HttpPut("{sectionId}/update-grid-view/{gridViewId}")]
        public async Task<IActionResult> UpdateGridView(
            Guid sectionId,
            [HashedKeyBinder(typeof(GridViewDTO))] int gridViewId,
            [FromBody] GridViewDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateGridView(sectionId, gridViewId, dto, ct));

        [HttpPut("{sectionId}/update-grid-view-column/{gridViewColumnId}")]
        public async Task<IActionResult> UpdateGridViewColumn(
            Guid sectionId,
            [HashedKeyBinder(typeof(GridViewColumnDTO))] int gridViewColumnId,
            [FromBody] GridViewColumnDTO dto,
            CancellationToken ct)
            => Ok(await _sectionService.UpdateGridViewColumn(sectionId, gridViewColumnId, dto, ct));
        #endregion
    }
}