using BBWM.DbDoc.Services;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;
public class MaxLengthValidationRule : ValidationRule
{
    public int MaxLength { get; set; }

    public override bool AcceptValidator(DbModelValidator validator, object value)
    {
        return validator.Validate(this, value);
    }
}
