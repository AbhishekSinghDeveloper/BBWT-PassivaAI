using BBWM.DbDoc.Services;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;

public class DateRangeValidationRule : RangeValidationRule<DateTimeOffset>
{
    public override bool AcceptValidator(DbModelValidator validator, object value)
    {
        if (value == null) return true;

        try
        {
            var date = Convert.ToDateTime(value);
            return validator.Validate(this, date);
        }
        catch
        {
            return false;
        }
    }
}
