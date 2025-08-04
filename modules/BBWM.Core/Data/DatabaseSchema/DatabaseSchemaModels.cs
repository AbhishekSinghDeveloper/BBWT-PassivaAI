namespace BBWM.Core.Data.DatabaseSchema;

public class DatabaseSchema
{
    public string SchemaName { get; set; }
    public IEnumerable<DatabaseSchemaTable> Tables { get; set; } = new List<DatabaseSchemaTable>();
}

public class DatabaseSchemaTable
{
    public string TableSchema { get; set; }
    public string TableName { get; set; }
    public int? TableRows { get; set; }
    public int? DataLength { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public string TableCollation { get; set; }

    /// <summary>
    /// Indicate is table entity is DB view.
    /// </summary>
    /// <remarks>
    /// TODO: in the future improvement, it's better to keep views as schema.Views
    /// </remarks>
    public bool IsView { get; set; }

    public DatabaseSchema DbSchema { get; set; }

    public IEnumerable<DatabaseSchemaColumn> Columns { get; set; } = new List<DatabaseSchemaColumn>();
}

public class DatabaseSchemaColumn
{
    public string ColumnName { get; set; }
    public int OrdinalPosition { get; set; }
    public string ColumnDefault { get; set; }
    public bool IsNullable { get; set; }
    public string DataType { get; set; }
    public long? CharacterMaximumLength { get; set; }
    public string ColumnType { get; set; }
    public string CharacterSetName { get; set; }
    public string CollationName { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    /// <summary>
    /// Implemented for MySQL DB schema reader only
    /// </summary>
    public bool IsUnique { get; set; }

    public DatabaseSchemaTable Table { get; set; }

    /// <summary>
    /// A list of key referencing to a source table column from this column as target
    /// </summary>
    public IEnumerable<DatabaseSchemaForeignKey> ContainingForeignKeys { get; set; } = new List<DatabaseSchemaForeignKey>();

    /// <summary>
    /// A list of keys referencing to this column as source from target columns
    /// </summary>
    public IEnumerable<DatabaseSchemaForeignKey> ReferencingForeignKeys { get; set; } = new List<DatabaseSchemaForeignKey>();
}

public class DatabaseSchemaForeignKey
{
    public string ConstraintName { get; set; }

    public DatabaseSchemaColumn SourceColumn { get; set; }

    public DatabaseSchemaColumn TargetColumn { get; set; }
}
