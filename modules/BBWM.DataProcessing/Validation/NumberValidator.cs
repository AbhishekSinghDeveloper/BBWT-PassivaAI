using BBWM.DataProcessing.Classes;

using System.Globalization;

namespace BBWM.DataProcessing.Validation;

/// <summary>
/// Validates the cell for number
/// </summary>
public class NumberValidator : EntryCellValidator
{
    private double _min = double.MinValue;
    private double _max = double.MaxValue;

    /// <summary>
    /// Custom constructor
    /// </summary>
    /// <param name="typeInfo">type info</param>
    public NumberValidator(NumberCellDataTypeInfo typeInfo = null)
    {
        if (typeInfo is null) return;
        _min = typeInfo.Min;
        _max = typeInfo.Max;
    }

    /// <summary>
    /// Perform validation for the cell value 
    /// </summary>
    /// <param name="cell">cell</param>
    public override void PerformValidation(ImportEntryCell cell)
    {
        double number;
        if (double.TryParse(Convert.ToString(cell.Value), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number))
        {
            cell.Value = number;
            PerformMinMaxValidation(cell);
        }
        else
        {
            cell.ErrorMessage = "The value is not a number";
        }
    }

    /// <summary>
    /// Perform validation for the cell value by min max
    /// </summary>
    /// <param name="cell">cell</param>
    protected void PerformMinMaxValidation(ImportEntryCell cell)
    {
        var number = Convert.ToDouble(cell.Value);

        if (_min > _max)
        {
            var tmp = _min;
            _min = _max;
            _max = tmp;
        }

        if (number < _min)
        {
            cell.ErrorMessage = $"The value is less than {_min}";
        }
        if (number > _max)
        {
            cell.ErrorMessage = $"The value is greater than {_max}";
        }
    }
}
