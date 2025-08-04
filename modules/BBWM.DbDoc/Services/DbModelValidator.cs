using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.Enums;

using PhoneNumbers;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BBWM.DbDoc.Services;

public class DbModelValidator
{
    public bool Validate(ValidationRule rule, object value)
    {
        return rule.AcceptValidator(this, value);
    }

    public bool Validate(RequiredValidationRule rule, object value)
    {
        return value != null;
    }

    public bool Validate(NumberRangeValidationRule rule, double value)
    {
        return ValidateRangeRule(rule, value);
    }

    public bool Validate(DateRangeValidationRule rule, DateTimeOffset value)
    {
        return ValidateRangeRule(rule, value);
    }

    private static bool ValidateRangeRule<T>(RangeValidationRule<T> rule, T value) where T : struct
    {
        return (!rule.Min.HasValue || Comparer<T>.Default.Compare(rule.Min.Value, value) <= 0) &&
               (!rule.Max.HasValue || Comparer<T>.Default.Compare(rule.Max.Value, value) >= 0);
    }

    public bool Validate(InputFormatValidationRule rule, string value)
    {
        switch (rule.Type)
        {
            case InputFormat.Phone:
                return ValidatePhone();
            case InputFormat.Email:
                return ValidateEmail();
            case InputFormat.Url:
                return ValidateUrl();
            case InputFormat.Regex:
                return ValidateByRegex(rule.Format);
            default:
                break;
        }

        return false;

        bool ValidatePhone()
        {
            var phoneNumber = Convert.ToString(value);
            var util = PhoneNumberUtil.GetInstance();

            try
            {
                var phone = util.Parse(phoneNumber, "GB");
                return util.IsValidNumber(phone);
            }
            catch
            {
                return false;
            }
        }

        bool ValidateEmail()
        {
            return new EmailAddressAttribute().IsValid(value);
        }

        bool ValidateUrl()
        {
            return Uri.TryCreate(Convert.ToString(value), UriKind.Absolute, out _);
        }

        bool ValidateByRegex(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(value);
        }
    }

    public bool Validate(MaxLengthValidationRule rule, object value)
    {
        return new MaxLengthAttribute(rule.MaxLength).IsValid(Convert.ToString(value));
    }
}
