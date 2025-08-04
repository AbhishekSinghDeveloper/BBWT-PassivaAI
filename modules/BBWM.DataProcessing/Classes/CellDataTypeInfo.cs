using System.Text.Json.Serialization;

namespace BBWM.DataProcessing.Classes;

/// <summary>
/// Additional info related with type
/// </summary>
public interface ICellDataTypeInfo
{ }

/// <summary>
/// Information for numeric type
/// </summary>
public class NumberCellDataTypeInfo : ICellDataTypeInfo
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="min">min value</param>
    /// <param name="max">max value</param>
    public NumberCellDataTypeInfo(double min, double max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public NumberCellDataTypeInfo()
    { }

    /// <summary>
    /// Min value
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Max value
    /// </summary>
    public double Max { get; set; }
}

/// <summary>
/// Information for DateTime type
/// </summary>
public class DateTimeCellDataTypeInfo : ICellDataTypeInfo
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="dateFormats">date format</param>
    public DateTimeCellDataTypeInfo(string dateFormats)
    {
        DateFormats = dateFormats;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public DateTimeCellDataTypeInfo()
    { }

    /// <summary>
    /// The format of date string
    /// </summary>
    public string DateFormats { get; set; }
}

/// <summary>
/// Information for custom type
/// </summary>
public class CustomCellDataTypeInfo : ICellDataTypeInfo
{
    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="customValidation">custom validation</param>
    public CustomCellDataTypeInfo(CustomValidationHandler customValidation)
    {
        CustomValidation = customValidation;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CustomCellDataTypeInfo()
    { }

    /// <summary>
    /// Custom validation predicate
    /// </summary>
    [JsonIgnore]
    public CustomValidationHandler CustomValidation { get; set; }
}