using BBWM.Core.Data.DatabaseSchema;
using BBWM.DbDoc.DbSchemas.Interfaces;

namespace BBWM.DbDoc.DbSchemas;

/// <summary>
/// This modifier allows to solve a problem of DB views missing FKs (in MySQL it's improssible to define references to
/// other tables from DB view). For specific project features based on DB DOC, we need query DB views automatically and
/// need to join them with main app DB tables (e.g. with Organizations - requirement comes from MiPro project).
/// Therefore we agreed on DB view column naming convention, which re-creates a "FK" to another table.
/// This FK is only an emulation for DB DOC level of course. But DB DOC based features treat it as real FK.
/// Format 1: FK_[TableName], where TableName - a referenced table, it's PK column is taking for referencing
/// Format 2: FK_[TableName]_[ColumnName], where TableName - a referenced table, [ColumnName] - a referenced column
///     of [TableName] table.
/// E.g. if DB view has a column "FK_Customers_DefOrgID" then we create a FK for the view to Customers->DefOrgID.
/// </summary>
public class ViewColumnRefModifier : IDatabaseSchemaModifier
{
    public DatabaseSchema ModifySchema(DatabaseSchema schema)
    {
        foreach (var table in schema.Tables)
        {
            if (table.IsView)
            {
                foreach (var column in table.Columns)
                {
                    const string fkPrefix = "FK_";

                    if (column.ColumnName.StartsWith(fkPrefix))
                    {
                        var viewFkColName = column.ColumnName[fkPrefix.Length..];

                        DatabaseSchemaTable pkTable = null;
                        DatabaseSchemaColumn pkColumn = null;

                        #region Try 1: Parsing format: FK_[TableName]
                        // Searching for a table with name fully matching the view column name
                        // supposing that PK column will be auto-detected from the PK table
                        pkTable = schema.Tables
                            .Where(x => viewFkColName.Equals(x.TableName, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        if (pkTable != null)
                        {
                            pkColumn = pkTable.Columns.FirstOrDefault(x => x.IsPrimaryKey);
                        }
                        #endregion

                        #region Try 2: Parsing format: FK_[TableName_ColumnName]
                        if (pkColumn == null)
                        {
                            const string pkColumnPrefix = "_";

                            // Searching for all tables with name matching the first part of the view column name,
                            // (before "_" separator) and ordering in table name length descending - to take the
                            // longest one, and so take into account a case when table name may contain "_"
                            // separator too.
                            // E.g. tables: "invoices", "invoices_details".
                            // DB View column: "FK_invoices_details_ID". The c will choose "invoices_details".
                            var matchingPkTables = schema.Tables
                                .Where(x => viewFkColName.StartsWith(x.TableName + pkColumnPrefix,
                                    StringComparison.OrdinalIgnoreCase))
                                .OrderByDescending(x => x.TableName.Length);

                            foreach (var matchedTable in matchingPkTables)
                            {
                                var pkColumnName = viewFkColName[(matchedTable.TableName + pkColumnPrefix).Length..];
                                pkColumn = matchedTable.Columns.FirstOrDefault(x =>
                                    x.ColumnName.Equals(pkColumnName, StringComparison.OrdinalIgnoreCase));

                                if (pkColumn != null)
                                    break;
                            }
                        }
                        #endregion

                        // If PK column found then adding FK references to schema
                        if (pkColumn != null)
                        {
                            var containingFKs = column.ContainingForeignKeys.ToList();
                            containingFKs.Add(new DatabaseSchemaForeignKey
                            {
                                ConstraintName = column.ColumnName,
                                SourceColumn = column,
                                TargetColumn = pkColumn
                            });
                            column.ContainingForeignKeys = containingFKs;

                            var referencingFKs = pkColumn.ReferencingForeignKeys.ToList();
                            referencingFKs.Add(new DatabaseSchemaForeignKey
                            {
                                ConstraintName = column.ColumnName,
                                SourceColumn = column,
                                TargetColumn = pkColumn
                            });
                            pkColumn.ReferencingForeignKeys = referencingFKs;

                            column.IsForeignKey = true;
                        }
                    }
                }
            }
        }

        return schema;
    }
}
