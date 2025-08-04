using BBWM.Core.Data;
using BBWM.Core.Data.DatabaseSchema;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using System.Collections.Immutable;

namespace BBWM.DbDoc.DbSchemas;

public class ConnectedDbSchemaReader : IDatabaseSchemaReader
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    private readonly IEnumerable<IDatabaseSchemaModifier> _schemaModifiers;

    private const int readerMaxRetryCount = 1;
    private const int readerMaxRetryDelay = 4;

    public ConnectedDbSchemaReader(string connectionString, DatabaseType databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
        _schemaModifiers = new List<IDatabaseSchemaModifier> { new ViewColumnRefModifier() };
    }

    /// <summary>
    /// Reads database schema by DB connection and converts to DB DOC's DB schema
    /// </summary>
    public async Task<DbSchema> ReadSchema(string schemaCode, IDbSchemaCodeValidator schemaCodeValidator,
        CancellationToken ct)
    {
        var schema = DbSchemaReaderFactory.CreateDbSchemaReader(_databaseType)
            .ReadDbSchema(_connectionString, readerMaxRetryCount, readerMaxRetryDelay);

        foreach (var modifier in _schemaModifiers)
        {
            schema = modifier.ModifySchema(schema);
        }

        schemaCode = FetchSchemaCode(schemaCode, schema, schemaCodeValidator);
        var dbDocSchema = ConvertToDbDocSchema(schemaCode, schema);
        return dbDocSchema;
    }

    private static string FetchSchemaCode(string schemaCode, DatabaseSchema schema,
        IDbSchemaCodeValidator schemaCodeValidator)
    {
        if (string.IsNullOrEmpty(schemaCode))
        {
            schemaCode = schema.SchemaName;

            var codePostfixCounter = 1;
            while (!schemaCodeValidator.IsSchemaCodeUnique(schemaCode, default).Result)
            {
                codePostfixCounter++;
                schemaCode = $"{schema.SchemaName}{codePostfixCounter}";
            }
        }
        return schemaCode;
    }

    private DbSchema ConvertToDbDocSchema(string schemaCode, DatabaseSchema schema)
    {
        var dbSchema = new DbSchema
        {
            SchemaCode = schemaCode,
            DatabaseName = schema.SchemaName
        };

        Dictionary<string, DbSchemaTable> tablesMap = new();
        Dictionary<string, DbSchemaColumn> columnsMap = new();

        foreach (var table in schema.Tables)
        {
            var dbSchemaTable = GetSchemaTable(table, dbSchema.SchemaCode);
            tablesMap[dbSchemaTable.TableId] = dbSchemaTable;

            foreach (var column in table.Columns)
            {
                var dbSchemaColumn = GetSchemaColumn(column, dbSchema.SchemaCode);                
                columnsMap[dbSchemaColumn.ColumnId] = dbSchemaColumn;
            }
        }

        dbSchema.Tables = tablesMap.ToImmutableSortedDictionary();
        dbSchema.Columns = columnsMap.ToImmutableSortedDictionary();

        return dbSchema;
    }

    private DbSchemaTable GetSchemaTable(DatabaseSchemaTable table, string schemaCode)
        => new()
        {
            TableId = GetTableId(table, schemaCode),
            TableName = table.TableName,
            DbName = table.DbSchema.SchemaName,
            Schema = table.TableSchema,
            QueryName = BuildTableQueryName(table),
            IsView = table.IsView
        };

    private DbSchemaColumn GetSchemaColumn(DatabaseSchemaColumn column, string schemaCode)
        => new()
        {
            ColumnId = GetColumnId(column, schemaCode),
            AllowNull = column.IsNullable,
            ColumnName = column.ColumnName,
            DefaultValue = column.ColumnDefault,
            IsForeignKey = column.IsForeignKey,
            ParentTableName = column.Table.TableName,
            PropertyName = column.ColumnName,
            IsPrimaryKey = column.IsPrimaryKey,
            QueryName = BuildColumnQueryName(column),
            TableId = GetTableId(column.Table, schemaCode),
            Type = column.ColumnType,
            TableReferences = GetSchemaTableReferences(column, schemaCode)
        };

    private static List<DbSchemaTableReference> GetSchemaTableReferences(DatabaseSchemaColumn column, string schemaCode)
    {
        var refs = new List<DbSchemaTableReference>();

        refs.AddRange(column.ContainingForeignKeys
            .Select(x => new DbSchemaTableReference
            {
                SourceTableId = GetTableId(x.SourceColumn.Table, schemaCode),
                SourceColumnId = GetColumnId(x.SourceColumn, schemaCode),
                TargetTableId = GetTableId(x.TargetColumn.Table, schemaCode),
                TargetColumnId = GetColumnId(x.TargetColumn, schemaCode),
                IsRequired = !x.SourceColumn.IsNullable
            }));

        refs.AddRange(column.ReferencingForeignKeys
            .Select(x => new DbSchemaTableReference
            {
                SourceTableId = GetTableId(x.SourceColumn.Table, schemaCode),
                SourceColumnId = GetColumnId(x.SourceColumn, schemaCode),
                TargetTableId = GetTableId(x.TargetColumn.Table, schemaCode),
                TargetColumnId = GetColumnId(x.TargetColumn, schemaCode),
                IsRequired = !x.SourceColumn.IsNullable
            }));

        return refs;
    }

    private string BuildTableQueryName(DatabaseSchemaTable table)
        => $"{table.DbSchema.SchemaName}" +
            $"{(string.IsNullOrEmpty(table.TableSchema) ? "" : ".")}{table.TableSchema}.{table.TableName}";

    private string BuildColumnQueryName(DatabaseSchemaColumn column)
        => $"{BuildTableQueryName(column.Table)}.{column.ColumnName}";

    private static string GetTableId(DatabaseSchemaTable table, string schemaCode)
        => schemaCode + "." + table.TableName;

    private static string GetColumnId(DatabaseSchemaColumn column, string schemaCode)
        => GetTableId(column.Table, schemaCode) + "." + column.ColumnName;

}