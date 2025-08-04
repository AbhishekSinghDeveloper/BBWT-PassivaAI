namespace BBWM.DataProcessing.Classes;

/// <summary>
/// Defines the column info for data import
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// The number of column in csv/xls file
    /// </summary>
    public int OrderNumber { get; set; }

    /// <summary>
    /// The name of target table's column
    /// </summary>
    public string TargetFieldName { get; set; }

    /// <summary>
    /// Cell data type
    /// </summary>
    public CellDataType Type { get; set; }

    /// <summary>
    /// Additional info related with type
    /// </summary>
    public ICellDataTypeInfo TypeInfo { get; set; }

    /// <summary>
    /// Column position
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Defines if the field allows null value
    /// </summary>
    public bool IsAllowNulls { get; set; }

    /// <summary>
    /// Default value if the field has null value
    /// </summary>
    public object DefaultValue { get; set; }
}
