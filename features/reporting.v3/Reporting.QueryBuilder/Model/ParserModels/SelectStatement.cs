namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class SelectStatement : SqlParserObject
{
    public QueryExpression? QueryExpression => Children.OfType<QueryExpression>().FirstOrDefault();
    public OrderByClause? OrderByClause => Children.OfType<OrderByClause>().FirstOrDefault();
    public LimitClause? LimitClause => Children.OfType<LimitClause>().FirstOrDefault();
}