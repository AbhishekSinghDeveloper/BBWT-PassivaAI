using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbGraph;

/// <summary>
/// The data inside vertices of a graph.
/// </summary>
public class DbTableInfo
{
    public DbSchemaColumn PrimaryKeyColumn { get; set; }

    public DbSchemaTable Table { get; set; }
}
