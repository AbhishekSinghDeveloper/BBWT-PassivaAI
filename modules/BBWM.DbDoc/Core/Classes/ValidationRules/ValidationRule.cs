using BBWM.DbDoc.Services;
using BBWM.DbDoc.Web;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Core.Classes.ValidationRules;

[JsonConverter(typeof(ValidationRuleConverter))]
public abstract class ValidationRule
{
    public bool IsSystem { get; set; }

    public string ErrorMessage { get; set; }

    public abstract bool AcceptValidator(DbModelValidator validator, object value);
}
