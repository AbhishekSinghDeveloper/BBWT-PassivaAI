using BBF.Reporting.Core.Enums;
using BBWM.Core.Data;

namespace BBF.Reporting.Core.DbModel;

public class VariableRule : IAuditableEntity
{
    public int Id { get; set; }
    public string VariableName { get; set; } = null!;
    public ExpressionOperator Operator { get; set; }
    public string? Operand { get; set; }
}