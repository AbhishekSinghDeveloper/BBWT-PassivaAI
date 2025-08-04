namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public abstract class LiteralExpression : SqlParserObject
{
    public string Value => Sql;
}