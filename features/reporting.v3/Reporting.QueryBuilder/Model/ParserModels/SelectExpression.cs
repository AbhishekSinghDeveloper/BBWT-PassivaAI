namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class SelectExpression : SqlParserObject
{
    public string? Alias { get; set; }
    public ColumnReference? Column => Children.OfType<ColumnReference>().FirstOrDefault();
    public FunctionCall? FunctionCall => Children.OfType<FunctionCall>().FirstOrDefault();
}