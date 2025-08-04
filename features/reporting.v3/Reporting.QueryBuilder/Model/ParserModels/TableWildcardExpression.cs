namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class TableWildcardExpression : SqlParserObject
{
    public string TableAlias { get; set; } = null!;
}