using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Interfaces;
public interface IQueryBuilderService
{
    Task<QueryTableJoin> AddDuplicateQueryTable(Query query, QueryTableJoinDTO join, CancellationToken ct = default);
    Task<QueryFilter> AddQueryFilter(QueryFilterDTO dto, CancellationToken ct = default);
    Task<QueryFilterSet> AddQueryFilterSet(int parentQueryFilterSetId, CancellationToken ct = default);
    Task<(QueryTable queryTable, IList<QueryTableJoin> joins)> AddQueryTable(Query query, int tableMetadataId, CancellationToken ct = default);
    Task<(QueryTableColumn queryTableColumn, IEnumerable<QueryTableJoin> joins)> AddQueryTableColumn(Query query, int columnMetadataId, int? parentQueryTableId, CancellationToken ct = default);
    Task<QueryTableJoin> AddQueryTableJoin(QueryTableJoinDTO joinDto, CancellationToken ct = default);
    Task<IList<QueryTable>> AddQueryTablesFromSource(Query query, QueryableTableSource[] sources, CancellationToken ct = default);
    Task<Query> CreateQuery(Guid dbDocFolderId, CancellationToken ct = default);
    Task DeleteQueryFilter(int queryFilterId, CancellationToken ct = default);
    Task DeleteQueryFilterSet(int queryFilterSetId, CancellationToken ct = default);
    Task DeleteQueryTable(int queryTableId, CancellationToken ct = default);
    Task DeleteQueryTableColumn(int queryTableColumnId, CancellationToken ct = default);
    Task DeleteQueryTableJoin(int queryTableJoinId, CancellationToken ct = default);
    IEnumerable<string> GetReachableTables(string uniqueTableId);
    Task<QueryFilterBinding> UpdateMasterDetailQueryFilterBinding(int queryFilterBindingId, QueryFilterBindingDTO dto, CancellationToken ct = default);
    Task<QueryFilter> UpdateQueryFilter(int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default);
    Task<QueryTableJoin> UpdateQueryTableJoin(QueryTableJoinDTO joinDto, CancellationToken ct = default);
    Task<QueryFilter> UpdateSqlFilter(int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default);
    Task<QueryFilterSet> UpdateQueryFilterSet(int queryFilterSetId, QueryFilterSetDTO dto, CancellationToken ct = default);
}
