using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbGraph;

/// <summary>
/// The data inside edges of a graph.
/// </summary>
public class DbTablesRelation
{
    public DbSchemaColumn StartTableColumn;

    public DbSchemaColumn EndTableColumn;

    public bool IsRequired;
}
