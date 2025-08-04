namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for CellDataTypeInfo
/// </summary>
public class CellDataTypeInfoDTO
{
    /// <summary>
    /// Min value
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Max value
    /// </summary>
    public double Max { get; set; }

    // <summary>
    /// The format of date string
    /// </summary>
    public string DateFormats { get; set; }

    /// <summary>
    /// Custom validation predicate
    /// </summary>
    public string CustomValidation { get; set; }
}
