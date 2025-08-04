using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum InputFormat
{
    [EnumMember(Value = "phone")] Phone,
    [EnumMember(Value = "email")] Email,
    [EnumMember(Value = "url")] Url,
    [EnumMember(Value = "regex")] Regex,
}
