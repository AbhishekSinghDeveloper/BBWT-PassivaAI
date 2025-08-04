namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class QuerySpecification : QueryExpression
{
    public SelectClause? SelectClause => Children.OfType<SelectClause>().FirstOrDefault();
    public FromClause? FromClause => Children.OfType<FromClause>().FirstOrDefault();
    public WhereClause? WhereClause => Children.OfType<WhereClause>().FirstOrDefault();
    public GroupByClause? GroupByClause => Children.OfType<GroupByClause>().FirstOrDefault();
}