using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.Reporting;

public static class DbDocExtensions
{
    public static string GetQueryAlias(this DbSchemaColumn column) =>
        $"{column.ParentTableName}_{column.ColumnName}";
}
