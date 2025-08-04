namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class GroupByClause : SqlParserObject
{
    public IEnumerable<ColumnReference> GroupByColumns => Children.OfType<ColumnReference>();
}