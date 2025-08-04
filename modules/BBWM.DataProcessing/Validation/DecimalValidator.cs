using BBWM.DataProcessing.Classes;

using System.Globalization;

namespace BBWM.DataProcessing.Validation;

public class DecimalValidator : NumberValidator
{
    public DecimalValidator(NumberCellDataTypeInfo typeInfo = null) : base(typeInfo)
    {
    }

    public override void PerformValidation(ImportEntryCell cell)
    {
        if (decimal.TryParse(Convert.ToString(cell.Value), NumberStyles.Any, NumberFormatInfo.CurrentInfo, out var number))
        {
            cell.Value = number;
            PerformMinMaxValidation(cell);
        }
        else
        {
            cell.ErrorMessage = "The value is not a decimal number";
        }
    }
}
