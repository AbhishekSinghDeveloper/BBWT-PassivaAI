namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class EmittedVariableReference : VariableReference
{
    public string Name => Sql[1..];
    public string Prefix => Sql[..1];
}