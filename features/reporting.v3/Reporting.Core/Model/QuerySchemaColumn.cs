using BBF.Reporting.Core.Enums;

namespace BBF.Reporting.Core.Model;

public class QuerySchemaColumn
{
    /// <summary>
    /// E.g. for column data type determination in widgets
    /// </summary>
    public DataType DataType { get; set; }

    public bool IsAliased { get; set; }
    public string QueryAlias { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public string ColumnName { get; set; } = null!;
    public string BaseTableName { get; set; } = null!;
    public string BaseColumnName { get; set; } = null!;
}