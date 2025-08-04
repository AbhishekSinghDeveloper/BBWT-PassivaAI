using BBWM.Core.Data;

namespace BBWM.Reporting.Model;

public class QueryTableColumn : IAuditableEntity
{
    public int Id { get; set; }

    public string SourceColumnId { get; set; }

    public bool OnlyForJoin { get; set; }


    public int QueryTableId { get; set; }

    public QueryTable QueryTable { get; set; }
}
