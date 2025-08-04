using BBWM.Core.Data;

namespace BBF.Reporting.Core.DbModel;

public class Variable : IAuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
