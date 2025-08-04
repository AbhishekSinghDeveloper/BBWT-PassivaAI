namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for ImportEntryCell
/// </summary>
public class ImportEntryCellDTO
{
    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// The name of target table's column
    /// </summary>
    public string TargetFieldName { get; set; }

    /// <summary>
    /// The number of column in csv/xls file
    /// </summary>
    public int OrderNumber { get; set; }

    /// <summary>
    /// The basic type of imported field
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; }
}
