namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class InExpression : WhereExpression
{
    public bool Negated { get; set; }
    public SqlParserObject? Left => Children.FirstOrDefault();
    public ListExpression? ListExpression => Children.Skip(1).OfType<ListExpression>().FirstOrDefault();
}