namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class SelectClause : SqlParserObject
{
    public bool Distinct { get; set; }
    public IEnumerable<SelectExpression> SelectExpressions => Children.OfType<SelectExpression>();
    public IEnumerable<WildcardExpression> Wildcards => Children.OfType<WildcardExpression>();
    public IEnumerable<TableWildcardExpression> TableWildcards => Children.OfType<TableWildcardExpression>();
}