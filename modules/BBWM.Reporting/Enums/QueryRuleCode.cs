using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum QueryRuleCode
    {
        [EnumMember(Value = "equals")] Equals,
        [EnumMember(Value = "notEquals")] NotEquals,
        [EnumMember(Value = "more")] More,
        [EnumMember(Value = "moreOrEqual")] MoreOrEqual,
        [EnumMember(Value = "less")] Less,
        [EnumMember(Value = "lessOrEqual")] LessOrEqual,
        [EnumMember(Value = "between")] Between,
        [EnumMember(Value = "contains")] Contains,
        [EnumMember(Value = "notContains")] NotContains,
        [EnumMember(Value = "startsWith")] StartsWith,
        [EnumMember(Value = "endsWith")] EndsWith
    }
}
