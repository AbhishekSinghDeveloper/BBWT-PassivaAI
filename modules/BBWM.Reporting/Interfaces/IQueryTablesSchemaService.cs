using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Interfaces;

/// <summary>
/// Build db schema tables for a query object, supporting multiple sources the query is joined from
/// (e.g. part of tables are from DB DOC folder and part of tables are DB views which wrap JSON data querying)
/// </summary>
public interface IQueryTablesSchemaService
{
    Task<QueryTablesSchema> BuildTablesSchema(Query query, CancellationToken ct);
}

public class QueryTablesSchema
{
    public IList<DbSchemaTable> Tables { get; set; }
    public IList<DbSchemaColumn> Columns { get; set; }
}
