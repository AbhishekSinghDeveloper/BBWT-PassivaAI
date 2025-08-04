using BBWM.Core.Web.JsonConverters;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBF.Reporting.Core.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum InputType
{
    [EnumMember(Value = "calendar")] Calendar,
    [EnumMember(Value = "checkbox")] Checkbox,
    [EnumMember(Value = "dropdown")] Dropdown,
    [EnumMember(Value = "multiselect")] Multiselect,
    [EnumMember(Value = "number")] Number,
    [EnumMember(Value = "text")] Text,
    [EnumMember(Value = "textarea")] Textarea
}
