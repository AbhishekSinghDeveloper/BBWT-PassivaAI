using BBWM.DbDoc.Enums;

namespace BBWM.DbDoc.DbSchemas.SchemaModels;

public class DbSchemaColumn
{
    public string ColumnId { get; set; }

    public string TableId { get; set; }

    public bool? AllowNull { get; set; }

    // TODO: to remove
    public ClrTypeGroup ClrTypeGroup { get; set; }

    public string ColumnName { get; set; }

    public string DefaultValue { get; set; }

    public string DefaultValueSql { get; set; }

    public bool? IsForeignKey { get; set; }

    public bool? IsIndex { get; set; }

    public bool? IsPrimaryKey { get; set; }

    public string ParentTableName { get; set; }

    public string PropertyName { get; set; }

    public string QueryName { get; set; }

    public string Type { get; set; }

    public IList<DbSchemaTableReference> TableReferences { get; set; } = new List<DbSchemaTableReference>();
}
