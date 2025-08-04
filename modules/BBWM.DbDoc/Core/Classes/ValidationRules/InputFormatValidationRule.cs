using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Services;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;

public class InputFormatValidationRule : ValidationRule
{
    public InputFormat Type { get; set; }

    public string Format { get; set; }


    public override bool AcceptValidator(DbModelValidator validator, object value)
    {
        if (value == null) return true;

        try
        {
            var str = Convert.ToString(value);
            return validator.Validate(this, str);
        }
        catch
        {
            return false;
        }
    }
}
