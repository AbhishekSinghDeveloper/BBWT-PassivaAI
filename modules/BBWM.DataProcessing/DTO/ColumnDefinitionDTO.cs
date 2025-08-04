using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for LookupListItem
/// </summary>
public class ColumnDefinitionDTO
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
    /// The basic type of imported field
    /// </summary>
    public CellDataType Type { get; set; }

    // <summary>
    /// Additional info related with type
    /// </summary>
    public CellDataTypeInfoDTO TypeInfo { get; set; }

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
    public string DefaultValue { get; set; }
}
