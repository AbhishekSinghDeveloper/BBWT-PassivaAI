using BBF.Reporting.QueryBuilder.Enums;

namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class QueryBinaryExpression : QueryExpression
{
    public QueryBinaryOperation Operation;
    public QueryExpression? Left => Children.OfType<QueryExpression>().FirstOrDefault();
    public QueryExpression? Right => Children.OfType<QueryExpression>().Skip(1).FirstOrDefault();
}