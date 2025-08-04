using System.Runtime.Serialization;
using BBWM.Core.Web.JsonConverters;
using System.Text.Json.Serialization;

namespace BBF.Reporting.Widget.Grid.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum DisplayMode
{
    [EnumMember(Value = "conditional")] Conditional,
    [EnumMember(Value = "date")] Date,
    [EnumMember(Value = "number")] Number,
    [EnumMember(Value = "text")] Text
}