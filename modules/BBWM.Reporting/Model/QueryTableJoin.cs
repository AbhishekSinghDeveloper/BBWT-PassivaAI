using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model;

public class QueryTableJoin : IAuditableEntity
{
    public int Id { get; set; }

    public Query Query { get; set; }

    public int QueryId { get; set; }

    public QueryTable FromQueryTable { get; set; }

    public int? FromQueryTableId { get; set; }

    public QueryTableColumn FromQueryTableColumn { get; set; }

    public int? FromQueryTableColumnId { get; set; }

    public QueryTable ToQueryTable { get; set; }

    public int? ToQueryTableId { get; set; }

    public QueryTableColumn ToQueryTableColumn { get; set; }

    public int? ToQueryTableColumnId { get; set; }

    public QueryJoinTypeEnum JoinType { get; set; }
}
