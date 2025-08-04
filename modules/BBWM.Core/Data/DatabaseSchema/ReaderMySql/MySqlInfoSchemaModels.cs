using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Core.Data.DatabaseSchema.ReaderMySql;

[Keyless]
public class MySqlInfoSchemaTable
{
    [Column("TABLE_SCHEMA")]
    public string TableSchema { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("TABLE_ROWS")]
    public int? TableRows { get; set; }

    [Column("DATA_LENGTH")]
    public int? DataLength { get; set; }

    [Column("CREATE_TIME")]
    public DateTime? CreateTime { get; set; }

    [Column("UPDATE_TIME")]
    public DateTime? UpdateTime { get; set; }

    [Column("TABLE_COLLATION")]
    public string TableCollation { get; set; }

    [Column("TABLE_TYPE")]
    public string TableType { get; set; }
}

[Keyless]
public class MySqlInfoSchemaColumn
{
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
    public long? CharacterMaximumLength { get; set; }

    [Column("CHARACTER_SET_NAME")]
    public string CharacterSetName { get; set; }

    [Column("COLLATION_NAME")]
    public string CollationName { get; set; }

    [Column("COLUMN_TYPE")]
    public string ColumnType { get; set; }

    [Column("COLUMN_KEY")]
    public string ColumnKey { get; set; }
}

[Keyless]
public class MySqlInfoSchemaKeyColumnUsage
{
    [Column("CONSTRAINT_NAME")]
    public string ConstraintName { get; set; }

    [Column("TABLE_NAME")]
    public string TableName { get; set; }

    [Column("COLUMN_NAME")]
    public string ColumnName { get; set; }

    [Column("REFERENCED_TABLE_NAME")]
    public string ReferencedTableName { get; set; }

    [Column("REFERENCED_COLUMN_NAME")]
    public string ReferencedColumnName { get; set; }
}