namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public abstract class SqlParserObject
{
    public Range Range { get; set; }
    public string Sql { get; set; } = null!;
    public SqlParserObject? Parent { get; set; }
    public List<SqlParserObject> Children { get; set; } = new();
}