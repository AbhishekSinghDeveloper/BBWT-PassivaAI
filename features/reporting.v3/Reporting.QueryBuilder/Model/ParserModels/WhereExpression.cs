namespace BBF.Reporting.QueryBuilder.Model.ParserModels;

public abstract class WhereExpression : SqlParserObject
{
}

public abstract class BinaryBooleanExpression : WhereExpression
{
    public WhereExpression? Left => Children.OfType<WhereExpression>().FirstOrDefault();
    public WhereExpression? Right => Children.OfType<WhereExpression>().Skip(1).FirstOrDefault();
}

public class AndExpression : BinaryBooleanExpression
{
}

public class OrExpression : BinaryBooleanExpression
{
}