using BBF.Reporting.Core.Enums;

namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class OrderByExpression : SqlParserObject
{
    public SortOrder? Direction { get; set; }
    public ColumnReference? Column => Children.OfType<ColumnReference>().FirstOrDefault();
}