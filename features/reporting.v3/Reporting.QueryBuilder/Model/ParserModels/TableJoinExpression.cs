namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class TableJoinExpression : TableExpression
{
    public TableExpression? Left => Children.OfType<TableExpression>().FirstOrDefault();
    public TableExpression? Right => Children.OfType<TableExpression>().Skip(1).FirstOrDefault();
    public ComparisonExpression? JoinCondition => Children.OfType<ComparisonExpression>().FirstOrDefault();
}