namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public class LimitClause : SqlParserObject
{
    public bool ExplicitNotation { get; set; }

    public NumericLiteral? Limit
        => ExplicitNotation
            ? Children.OfType<NumericLiteral>().FirstOrDefault()
            : Children.OfType<NumericLiteral>().Skip(1).FirstOrDefault();

    public NumericLiteral? Offset
        => ExplicitNotation
            ? Children.OfType<NumericLiteral>().Skip(1).FirstOrDefault()
            : Children.OfType<NumericLiteral>().FirstOrDefault();
}