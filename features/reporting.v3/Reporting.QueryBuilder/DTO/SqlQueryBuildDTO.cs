using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.QueryBuilder.DTO;

public class SqlQueryBuildDTO : IDTO
{
    public int Id { get; set; }
    public string SqlCode { get; set; } = null!;

    // Foreign keys and navigational properties.
    public int? TableSetId { get; set; }
    public Guid QuerySourceId { get; set; }

    public QuerySourceDTO QuerySource { get; set; } = null!;
}