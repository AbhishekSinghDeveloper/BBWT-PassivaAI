using BBWM.DbDoc.Services;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;

public class RequiredValidationRule : ValidationRule
{
    public override bool AcceptValidator(DbModelValidator validator, object value) =>
        validator.Validate(this, value);
}
