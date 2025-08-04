namespace BBWM.DbDoc.DbSchemas.SchemaModels;

/// <summary>
/// Represents a reference on a table.
/// </summary>
public class DbSchemaTableReference
{
    public string SourceTableId { get; set; }

    public string SourceColumnId { get; set; }

    public string TargetTableId { get; set; }

    public string TargetColumnId { get; set; }

    public bool IsRequired { get; set; }
}
