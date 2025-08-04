using BBF.Reporting.Core.DbModel;
using BBWM.Core.Data;

namespace BBF.Reporting.QueryBuilder.DbModel;

public class SqlQuery : IAuditableEntity
{
    public int Id { get; set; }
    public string SqlCode { get; set; } = null!;

    // Foreign keys and navigational properties.
    public Guid QuerySourceId { get; set; }

    public QuerySource QuerySource { get; set; } = null!;

    /// <summary>
    /// Table set populated into the tables selector (as tables tree) of the query builder
    /// </summary>
    public int? TableSetId { get; set; }

    public TableSet.DbModel.TableSet? TableSet { get; set; }
}