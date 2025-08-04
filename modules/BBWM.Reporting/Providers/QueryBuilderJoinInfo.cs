namespace BBWM.Reporting.Providers;

internal class QueryBuilderJoinInfo
{
    public string FromTableQueryName { get; set; }

    public string FromColumnQueryName { get; set; }

    public string ToTableQueryName { get; set; }

    public string ToColumnQueryName { get; set; }

    public bool IsRequired { get; set; }
}
