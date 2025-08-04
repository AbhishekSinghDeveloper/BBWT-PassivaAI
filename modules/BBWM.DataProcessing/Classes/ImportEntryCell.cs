namespace BBWM.DataProcessing.Classes;

/// <summary>
/// The data import cell
/// </summary>
public class ImportEntryCell
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="originalValue">Original value</param>
    /// <param name="columnDefinition">Column definition</param>
    public ImportEntryCell(object originalValue, ColumnDefinition columnDefinition)
    {
        OriginalValue = originalValue;
        Value = originalValue;
        ColumnDefinition = columnDefinition;
    }

    /// <summary>
    /// The Column definition related to this cell 
    /// </summary>
    public ColumnDefinition ColumnDefinition { get; private set; }

    /// <summary>
    /// The cell's original value
    /// </summary>
    public object OriginalValue { get; private set; }

    /// <summary>
    /// The cell's current post validated value
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// The validation error message
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The name of target table's column
    /// </summary>
    public string TargetFieldName { get { return ColumnDefinition.TargetFieldName; } }

    /// <summary>
    /// The number of column in csv/xls file
    /// </summary>
    public int OrderNumber { get { return ColumnDefinition.OrderNumber; } }

    /// <summary>
    /// The basic type of imported field
    /// </summary>
    public int Type { get { return (int)ColumnDefinition.Type; } }

    /// <summary>
    /// Determines if the cell is valid
    /// </summary>
    public bool IsValid
    {
        get { return string.IsNullOrWhiteSpace(ErrorMessage); }
    }
}
