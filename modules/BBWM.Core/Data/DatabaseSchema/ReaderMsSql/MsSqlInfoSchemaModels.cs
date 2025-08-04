using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMsSql;

[Keyless]
public class MsSqlInfoSchemaTable
{
    [Column("TABLE_CATALOG")]
    public string TableCatalog { get; set; }

    [Column("TABLE_SCHEMA")]
    public string TableSchema { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("TABLE_TYPE")]
    public string TableType { get; set; }
}

[Keyless]
public class MsSqlInfoSchemaColumn
{
    [Column("TABLE_CATALOG")]
    public string TableCatalog { get; set; }

    [Column("TABLE_SCHEMA")]
    public string TableSchema { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; }

    [Column("ORDINAL_POSITION")]
    public int OrdinalPosition { get; set; }

    [Column("COLUMN_DEFAULT")]
    public string ColumnDefault { get; set; }

    [Column("IS_NULLABLE")]
    public string IsNullable { get; set; }

    [Column("DATA_TYPE")]
    public string DataType { get; set; }

    [Column("CHARACTER_MAXIMUM_LENGTH")]
    public int? CharacterMaximumLength { get; set; }

    [Column("CHARACTER_SET_NAME")]
    public string CharacterSetName { get; set; }

    [Column("COLLATION_NAME")]
    public string CollationName { get; set; }
}

[Keyless]
public class MsSqlInfoSchemaTableConstraints
{
    [Column("CONSTRAINT_NAME")]
    public string ConstraintName { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("CONSTRAINT_TYPE")]
    public string ConstraintType { get; set; }

    public bool IsPrimaryKey => ConstraintType == "PRIMARY KEY";
    public bool IsForeignKey => ConstraintType == "FOREIGN KEY";
}

[Keyless]
public class MsSqlInfoSchemaKeyColumnUsage
{
    [Column("CONSTRAINT_NAME")]
    public string ConstraintName { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; }
}

[Keyless]
public class MsSqlInfoSchemaReferentialConstraints
{
    [Column("CONSTRAINT_NAME")]
    public string ConstraintName { get; set; }

    [Column("UNIQUE_CONSTRAINT_NAME")]
    public string UniqueConstraintName { get; set; }
}