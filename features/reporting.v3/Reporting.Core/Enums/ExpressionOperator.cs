using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBF.Reporting.Core.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum ExpressionOperator
{
    [EnumMember(Value = "equals")] Equals = 0,
    [EnumMember(Value = "notEquals")] NotEquals = 1,
    [EnumMember(Value = "more")] More = 2,
    [EnumMember(Value = "moreOrEqual")] MoreOrEqual = 3,
    [EnumMember(Value = "less")] Less = 4,
    [EnumMember(Value = "lessOrEqual")] LessOrEqual = 5,
    [EnumMember(Value = "between")] Between = 6,
    [EnumMember(Value = "contains")] Contains = 7,
    [EnumMember(Value = "notContains")] NotContains = 8,
    [EnumMember(Value = "startsWith")] StartsWith = 9,
    [EnumMember(Value = "endsWith")] EndsWith = 10,
    [EnumMember(Value = "in")] In = 11,
    [EnumMember(Value = "notIn")] NotIn = 12,
    [EnumMember(Value = "isSet")] IsSet = 13
}