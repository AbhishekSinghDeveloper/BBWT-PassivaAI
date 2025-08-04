using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum QueryRuleDataType
    {
        [EnumMember(Value = "string")] String,
        [EnumMember(Value = "numeric")] Numeric,
        [EnumMember(Value = "boolean")] Boolean,
        [EnumMember(Value = "datetime")] Datetime
    }
}
