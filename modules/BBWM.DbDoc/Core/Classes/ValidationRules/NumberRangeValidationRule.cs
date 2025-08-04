using BBWM.DbDoc.Services;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;

public class NumberRangeValidationRule : RangeValidationRule<double>
{
    public override bool AcceptValidator(DbModelValidator validator, object value)
    {
        if (value == null) return true;

        try
        {
            var number = Convert.ToDouble(value);
            return validator.Validate(this, number);
        }
        catch
        {
            return false;
        }
    }
}
