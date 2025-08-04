using BBWM.Core.Web.JsonConverters;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum QueryJoinTypeEnum
{
    [EnumMember(Value = "inner")] Inner,
    [EnumMember(Value = "left")] Left
}
