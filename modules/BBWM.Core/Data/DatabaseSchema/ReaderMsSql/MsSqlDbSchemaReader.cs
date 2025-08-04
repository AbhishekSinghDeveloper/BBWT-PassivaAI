using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMsSql;

public class MsSqlDbSchemaReader : IDbSchemaReader
{
    public DatabaseSchema ReadDbSchema(string connectionString, int maxRetryCount = 0, int maxRetryDelay = 0)
    {
        var builder = new DbContextOptionsBuilder<MsSqlDbSchemaContext>();

        if (maxRetryCount > 0)
            builder.UseSqlServer(connectionString,
                builder => builder.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelay), null));
        else
            builder.UseSqlServer(connectionString);

        var ctx = new MsSqlDbSchemaContext(builder.Options);
        return ConvertContextDataToDbSchema(ctx);
    }

    private static DatabaseSchema ConvertContextDataToDbSchema(MsSqlDbSchemaContext ctx)
    {
        var infTables = ctx.InfoSchemaTables.ToList();
        var infTableConstraints = ctx.InfoSchemaTableConstraints.ToList();
        var infKeyColumnUsage = ctx.InfoSchemaKeyColumnUsage.ToList();
        var infColumns = ctx.InfoSchemaColumns.ToList();
        var infReferentialConstraints = ctx.InfoSchemaReferentialConstraints.ToList();

        var schema = new DatabaseSchema { SchemaName = ctx.DbSchemaName };
        schema.Tables = infTables.ConvertAll(x =>
                new DatabaseSchemaTable
                {
                    DbSchema = schema,
                    TableSchema = x.TableSchema,
                    TableName = x.TableName,
                    IsView = x.TableType?.ToLowerInvariant() == "view"
                });

        foreach (var table in schema.Tables)
        {
            table.Columns = infColumns.Where(y => y.TableName == table.TableName)
                .Select(x => new DatabaseSchemaColumn
                {
                    Table = table,
                    ColumnName = x.ColumnName,
                    OrdinalPosition = x.OrdinalPosition,
                    ColumnDefault = x.ColumnDefault,
                    IsNullable = x.IsNullable?.ToLowerInvariant() == "yes",
                    DataType = x.DataType,
                    CharacterMaximumLength = x.CharacterMaximumLength,
                    CharacterSetName = x.CharacterSetName,
                    CollationName = x.CollationName,
                    ColumnType = x.DataType,
                    IsPrimaryKey = infTableConstraints.Any(y => y.IsPrimaryKey && infKeyColumnUsage.Any(
                        w => w.ConstraintName == y.ConstraintName && w.TableName == table.TableName && w.ColumnName == x.ColumnName))
                })
                .ToList();
        }

        var allColumns = schema.Tables.SelectMany(x => x.Columns);

        #region set up foreing keys
        foreach (var rc in infReferentialConstraints)
        {
            var fk = new DatabaseSchemaForeignKey
            {
                ConstraintName = rc.ConstraintName,
                SourceColumn = allColumns.FirstOrDefault(x => infKeyColumnUsage.Any(y =>
                    y.ConstraintName == rc.ConstraintName && y.TableName == x.Table.TableName && y.ColumnName == x.ColumnName)),
                TargetColumn = allColumns.FirstOrDefault(x => infKeyColumnUsage.Any(y =>
                    y.ConstraintName == rc.UniqueConstraintName && y.TableName == x.Table.TableName && y.ColumnName == x.ColumnName)),
            };

            ((List<DatabaseSchemaForeignKey>)fk.SourceColumn.ContainingForeignKeys).Add(fk);
            ((List<DatabaseSchemaForeignKey>)fk.TargetColumn.ReferencingForeignKeys).Add(fk);
        }

        foreach (var column in allColumns)
        {
            column.IsForeignKey = column.ContainingForeignKeys.Any();
        }
        #endregion

        return schema;
    }
}
