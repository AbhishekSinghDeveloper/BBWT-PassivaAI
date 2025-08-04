using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO;

public class QueryTableJoinDTO : IDTO
{
    public int Id { get; set; }

    public QueryDTO Query { get; set; }

    public int QueryId { get; set; }

    public string FromDbDocColumnId { get; set; }

    public string FromDbDocTableId { get; set; }

    public QueryTableDTO FromQueryTable { get; set; }

    public int? FromQueryTableId { get; set; }

    public QueryTableColumnDTO FromQueryTableColumn { get; set; }

    public int? FromQueryTableColumnId { get; set; }

    public string ToDbDocColumnId { get; set; }

    public string ToDbDocTableId { get; set; }

    public QueryTableDTO ToQueryTable { get; set; }

    public int? ToQueryTableId { get; set; }

    public QueryTableColumnDTO ToQueryTableColumn { get; set; }

    public int? ToQueryTableColumnId { get; set; }

    public QueryJoinTypeEnum JoinType { get; set; }
}
