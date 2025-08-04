using BBWM.Core.Web.JsonConverters;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBF.Reporting.Core.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum DataType
{
    [EnumMember(Value = "numeric")] Numeric,
    [EnumMember(Value = "string")] String,
    [EnumMember(Value = "date")] Date,
    [EnumMember(Value = "other")] Other,
    [EnumMember(Value = "bool")] Bool
}
