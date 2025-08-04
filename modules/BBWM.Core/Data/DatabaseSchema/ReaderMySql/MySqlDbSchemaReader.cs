using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMySql;

public class MySqlDbSchemaReader : IDbSchemaReader
{
    public DatabaseSchema ReadDbSchema(string connectionString, int maxRetryCount = 0, int maxRetryDelay = 0)
    {
        var builder = new DbContextOptionsBuilder<MySqlDbSchemaContext>();

        if (maxRetryCount > 0)
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    builder => builder.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelay), null));
        else
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        var ctx = new MySqlDbSchemaContext(builder.Options);
        return ConvertContextDataToDbSchema(ctx);

    }

    private static DatabaseSchema ConvertContextDataToDbSchema(MySqlDbSchemaContext ctx)
    {
        var infTables = ctx.InfoSchemaTables.ToList();
        var infColumns = ctx.InfoSchemaColumns.ToList();
        var infForeignKeys = ctx.InfoSchemaKeyColumnUsage.ToList();

        var schema = new DatabaseSchema { SchemaName = ctx.DbSchemaName };
        schema.Tables = infTables.ConvertAll(x =>
                new DatabaseSchemaTable
                {
                    DbSchema = schema,
                    TableName = x.TableName,
                    TableRows = x.TableRows,
                    DataLength = x.DataLength,
                    CreateTime = x.CreateTime,
                    UpdateTime = x.UpdateTime,
                    TableCollation = x.TableCollation,
                    IsView = x.TableType?.ToLowerInvariant() == "view",
                });

        foreach (var table in schema.Tables)
        {
            table.Columns = infColumns.Where(y => y.TableName == table.TableName)
                .Select(y => new DatabaseSchemaColumn
                {
                    Table = table,
                    ColumnName = y.ColumnName,
                    OrdinalPosition = y.OrdinalPosition,
                    ColumnDefault = y.ColumnDefault,
                    IsNullable = y.IsNullable?.ToLowerInvariant() == "yes",
                    DataType = y.DataType,
                    CharacterMaximumLength = y.CharacterMaximumLength,
                    CharacterSetName = y.CharacterSetName,
                    CollationName = y.CollationName,
                    ColumnType = y.ColumnType,
                    IsPrimaryKey = y.ColumnKey?.ToLowerInvariant() == "pri",
                    IsUnique = y.ColumnKey?.ToLowerInvariant() == "uni",
                })
                .ToList();
        }

        var allColumns = schema.Tables.SelectMany(x => x.Columns);

        #region set up foreing keys
        foreach (var table in schema.Tables)
        {
            foreach (var column in table.Columns)
            {
                column.ContainingForeignKeys = infForeignKeys
                    .Where(x => x.TableName == column.Table.TableName && x.ColumnName == column.ColumnName)
                    .Select(x => new DatabaseSchemaForeignKey
                    {
                        ConstraintName = x.ConstraintName,
                        SourceColumn = column,
                        TargetColumn = allColumns.SingleOrDefault(y => y.Table.TableName == x.ReferencedTableName
                            && y.ColumnName == x.ReferencedColumnName)
                    })
                    .ToList();

                column.ReferencingForeignKeys = infForeignKeys
                    .Where(x => x.ReferencedTableName == column.Table.TableName && x.ReferencedColumnName == column.ColumnName)
                    .Select(x => new DatabaseSchemaForeignKey
                    {
                        ConstraintName = x.ConstraintName,
                        TargetColumn = column,
                        SourceColumn = allColumns.SingleOrDefault(y => y.Table.TableName == x.TableName
                            && y.ColumnName == x.ColumnName)
                    })
                    .ToList();

                column.IsForeignKey = column.ContainingForeignKeys.Any();
            }
        }
        #endregion

        return schema;
    }
}
