using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBF.Reporting.QueryBuilder.Model;

public class TablesRelation
{
    public DbSchemaColumn StartTableColumn { get; set; } = null!;
    public DbSchemaColumn EndTableColumn { get; set; } = null!;
}