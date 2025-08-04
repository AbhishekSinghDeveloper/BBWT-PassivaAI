using BBF.Reporting.QueryBuilder.Enums;

namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class ComparisonExpression : WhereExpression
{
    public ComparisonOperation Operation { get; set; }
    public SqlParserObject? Left => Children.FirstOrDefault();
    public SqlParserObject? Right => Children.Skip(1).FirstOrDefault();
}