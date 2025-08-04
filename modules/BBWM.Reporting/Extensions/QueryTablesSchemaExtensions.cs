using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Extensions;

public static class QueryTablesSchemaExtensions
{
    /// <summary>
    /// Gets DB schema column corresponding to a query's table column
    /// </summary>
    public static DbSchemaColumn GetColumn(this QueryTablesSchema schema, QueryTableColumn queryTableColumn) =>
        schema.Columns.FirstOrDefault(x =>
            x.ColumnId == queryTableColumn.SourceColumnId
            && x.TableId == queryTableColumn.QueryTable.SourceTableId);

    /// <summary>
    /// Gets DB schema column corresponding to a query column's source column ID
    /// </summary>
    public static DbSchemaColumn GetColumn(this QueryTablesSchema schema, string sourceTableId, string sourceColumnId) =>
        schema.Columns.FirstOrDefault(x => x.ColumnId == sourceColumnId && x.TableId == sourceTableId);

    /// <summary>
    /// Gets DB schema table corresponding to a query's table
    /// </summary>
    public static DbSchemaTable GetTable(this QueryTablesSchema schema, QueryTable queryTable) =>
        schema.Tables.FirstOrDefault(x => x.TableId == queryTable.SourceTableId);

    /// <summary>
    /// Gets DB schema table corresponding a query table's source table ID
    /// </summary>
    public static DbSchemaTable GetTable(this QueryTablesSchema schema, string sourceTableId) =>
        schema.Tables.FirstOrDefault(x => x.TableId == sourceTableId);
}