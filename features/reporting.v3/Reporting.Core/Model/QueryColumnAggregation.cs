namespace BBF.Reporting.Core.Model;

public class QueryColumnAggregation
{
    public string QueryAlias { get; set; } = null!;
    public IList<string> Expressions { get; set; } = null!;
}