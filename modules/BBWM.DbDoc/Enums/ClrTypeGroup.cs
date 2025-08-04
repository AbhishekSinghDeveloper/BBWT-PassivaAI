using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum ClrTypeGroup
{
    [EnumMember(Value = "numeric")] Numeric,
    [EnumMember(Value = "string")] String,
    [EnumMember(Value = "date")] Date,
    [EnumMember(Value = "other")] Other,
    [EnumMember(Value = "bool")] Bool
}
