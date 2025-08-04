using BBF.Reporting.Core.Enums;
using BBWM.Core.DTO;

namespace BBF.Reporting.Core.DTO;

public class FilterRuleDTO : IDTO
{
    public int Id { get; set; }
    public string Operand { get; set; } = null!;
    public ExpressionOperator Operator { get; set; }
    public string TableColumnId { get; set; } = null!;
}