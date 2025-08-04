namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class OrderByClause : SqlParserObject
{
    public IEnumerable<OrderByExpression> OrderByExpressions => Children.OfType<OrderByExpression>();
}