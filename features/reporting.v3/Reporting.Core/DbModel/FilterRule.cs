using BBF.Reporting.Core.Enums;

namespace BBF.Reporting.Core.DbModel;

public class FilterRule
{
    public int Id { get; set; }
    public string Operand { get; set; } = null!;
    public ExpressionOperator Operator { get; set; }
    public string TableColumnId { get; set; } = null!;
}