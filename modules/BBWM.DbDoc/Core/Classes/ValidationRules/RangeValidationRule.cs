namespace BBWM.DbDoc.Core.Classes.ValidationRules;

public abstract class RangeValidationRule<T> : ValidationRule where T : struct
{
    public T? Min { get; set; }
    public T? Max { get; set; }
}
