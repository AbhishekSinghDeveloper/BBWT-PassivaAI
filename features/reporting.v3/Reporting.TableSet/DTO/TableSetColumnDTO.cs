using BBWM.Core.DTO;

namespace BBF.Reporting.TableSet.DTO;

public class TableSetColumnDTO : IDTO<string>
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string TableId { get; set; } = null!;
    public string ColumnAlias { get; set; } = null!;
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
}