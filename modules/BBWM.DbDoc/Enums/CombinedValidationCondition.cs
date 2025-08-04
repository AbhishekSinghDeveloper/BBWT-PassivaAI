using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum CombinedValidationCondition
{
    [EnumMember(Value = "and")] And,
    [EnumMember(Value = "or")] Or
}
