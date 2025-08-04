using BBWM.Core.Data;

namespace BBWM.Reporting.Model;

public class QueryTable : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Identifier of a table schema in context of the schema's source provider.
    /// For DB DOC source provider it's a TableId of DB DOC table metadata record (e.g. "DataContext.Addresses")
    /// </summary>
    public string SourceTableId { get; set; }

    /// <summary>
    /// A short code representing table schema's source. By default it's an empty string (DB DOC).
    /// Another example "form" of the Forms module.
    /// </summary>
    public string SourceCode { get; set; }

    public string SelfJoinDbDocColumnId { get; set; }

    public string Alias { get; set; }

    public bool OnlyForJoin { get; set; }


    public int QueryId { get; set; }

    public Query Query { get; set; }


    public IList<QueryTableColumn> Columns { get; set; } = new List<QueryTableColumn>();
}
