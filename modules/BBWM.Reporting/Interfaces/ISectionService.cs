using BBWM.DbDoc.DTO;
using BBWM.Reporting.DTO;

namespace BBWM.Reporting.Interfaces;

public interface ISectionService
{
    Task<ReportChangeResult> AddDuplicateQueryTable(Guid sectionId, QueryTableJoinDTO join, CancellationToken ct = default);
    Task<ReportChangeResult> AddFilterControl(Guid sectionId, FilterControlDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> AddQueryFilter(Guid sectionId, QueryFilterDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> AddQueryFilterSet(Guid sectionId, int parentQueryFilterSetId, CancellationToken ct = default);
    Task<ReportChangeResult> AddQueryTable(Guid sectionId, int tableMetadataId, CancellationToken ct = default);
    Task<ReportChangeResult> AddQueryTableJoin(Guid sectionId, QueryTableJoinDTO joinDto, CancellationToken ct = default);
    Task<ReportChangeResult> AddQueryTablesFromSource(Guid sectionId, QueryableTableSource[] sources, CancellationToken ct);
    Task<ReportChangeResult> AddQueryTableColumn(Guid sectionId, int columnMetadataId, int? parentQueryTableId = null, CancellationToken ct = default);
    Task<ReportChangeResult> BindFilterControlToQueryFilter(Guid sectionId, int filterControlId, int queryFilterId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryTableColumn(Guid sectionId, int queryTableColumnId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteFilterControl(Guid sectionId, int filterControlId, bool deleteLinkedQueryFilters, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryFilter(Guid sectionId, int queryFilterId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryFilterBinding(Guid sectionId, int filterControlBindingId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryFilterSet(Guid sectionId, int queryFilterSetId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryTable(Guid sectionId, int queryTableId, CancellationToken ct = default);
    Task<ReportChangeResult> DeleteQueryTableJoin(Guid sectionId, int queryTableJoinId, CancellationToken ct = default);
    Task<bool> Exists(Guid sectionId, CancellationToken ct = default);
    Task<IEnumerable<TableMetadataDTO>> GetSectionTablesMatadata(Guid sectionId, CancellationToken ct = default);
    Task<QueryDTO> GetQueryStructure(Guid sectionId, CancellationToken ct = default);
    Task<IEnumerable<string>> GetReachableTables(Guid sectionId, CancellationToken ct = default);
    Task<string> GetSqlQuery(Guid sectionId, bool reduceSyntax = false, CancellationToken ct = default);
    Task<ViewDTO> GetView(Guid sectionId, CancellationToken ct = default);
    Task ToggleSectionGridViewColumnsSortable(Guid sectionId, bool value, CancellationToken ct = default);
    Task ToggleSectionGridViewColumnsVisible(Guid sectionId, bool value, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateFilterControl(Guid sectionId, int filterControlId, FilterControlDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateGridView(Guid sectionId, int gridViewId, GridViewDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateGridViewColumn(Guid sectionId, int gridViewColumnId, GridViewColumnDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateMasterDetailQueryFilterBinding(Guid sectionId, QueryFilterBindingDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateQueryFilter(Guid sectionId, int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateSqlFilter(Guid sectionId, int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateQueryFilterSet(Guid sectionId, int queryFilterSetId, QueryFilterSetDTO dto, CancellationToken ct = default);
    Task<ReportChangeResult> UpdateQueryTableJoin(Guid sectionId, QueryTableJoinDTO joinDto, CancellationToken ct = default);
}
