namespace BBWM.Reporting.Providers;

public class QueryBuilderNode
{
    public string TableName { get; set; }

    public string MyColumnName { get; set; }

    public string ParentColumnName { get; set; }

    public bool IsRequired { get; set; }

    public IEnumerable<string> Columns { get; set; }

    public IList<QueryBuilderNode> Children { get; set; } = new List<QueryBuilderNode>();
}
