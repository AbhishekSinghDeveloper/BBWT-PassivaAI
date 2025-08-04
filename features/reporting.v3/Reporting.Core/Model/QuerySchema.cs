namespace BBF.Reporting.Core.Model;

public class QuerySchema
{
    /// <summary>
    /// List of columns representing the query's table format output
    /// </summary>
    public IEnumerable<QuerySchemaColumn> Columns { get; set; } = new List<QuerySchemaColumn>();
}
