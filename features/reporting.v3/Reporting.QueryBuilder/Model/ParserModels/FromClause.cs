namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class FromClause : SqlParserObject
{
    public IEnumerable<TableExpression> TableExpressions => Children.OfType<TableExpression>();
}