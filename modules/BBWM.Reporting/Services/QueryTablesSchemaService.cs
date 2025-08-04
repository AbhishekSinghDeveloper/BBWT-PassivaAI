using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Services;

public class QueryTablesSchemaService : IQueryTablesSchemaService
{
    private readonly IQueryableTableSourceService tableSourceService;
    private readonly IDbSchemaManager dbSchemaManager;

    public QueryTablesSchemaService(
        IQueryableTableSourceService tableSourceService,
        IDbSchemaManager dbSchemaManager
        )
    {
        this.tableSourceService = tableSourceService;
        this.dbSchemaManager = dbSchemaManager;
    }

    public async Task<QueryTablesSchema> BuildTablesSchema(Query query, CancellationToken ct)
    {
        var queryTablesSchema = new QueryTablesSchema
        {
            Tables = new List<DbSchemaTable>(),
            Columns = new List<DbSchemaColumn>(),
        };

        foreach (var table in query.QueryTables)
        {
            // Get schema table & columns from DB DOC database source's DB schema
            if (string.IsNullOrEmpty(table.SourceCode))
            {
                queryTablesSchema.Tables.Add(dbSchemaManager.GetTable(table.SourceTableId));

                var columns = dbSchemaManager.GetTableColumns(table.SourceTableId);
                foreach (var column in columns)
                    queryTablesSchema.Columns.Add(column);
            }
            // Get schema table & columns from queryable table source's provider determined by table's source code
            else
            {
                //TODO: here we fetch a queryable table's record each time from the whole list of tables of the source
                // and so we fetch the whole source multiple times. Instead we should request a particular table once
                // from the source's provider.
                var tableSource = await tableSourceService.GetQueryableTableSource(table.SourceCode, ct);
                var schemaTable = tableSource.Tables.FirstOrDefault(x => x.SchemaTable.TableId == table.SourceTableId);
                queryTablesSchema.Tables.Add(schemaTable.SchemaTable);

                foreach (var column in schemaTable.SchemaColumns)
                    queryTablesSchema.Columns.Add(column);
            }
        }

        return queryTablesSchema;
    }
}
