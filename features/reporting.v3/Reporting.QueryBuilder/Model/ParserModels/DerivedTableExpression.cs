namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class DerivedTableExpression : TableExpression
{
    public QueryExpression? QueryExpression => Children.OfType<QueryExpression>().FirstOrDefault();
}