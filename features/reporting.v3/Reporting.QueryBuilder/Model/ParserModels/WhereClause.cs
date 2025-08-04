namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class WhereClause : SqlParserObject
{
    public IEnumerable<WhereExpression> WhereExpressions => Children.OfType<WhereExpression>();
}