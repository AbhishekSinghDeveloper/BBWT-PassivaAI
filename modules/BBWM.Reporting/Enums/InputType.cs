using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum InputType
    {
        [EnumMember(Value = "text")] Text,
        [EnumMember(Value = "number")] Number,
        [EnumMember(Value = "checkbox")] Checkbox,
        [EnumMember(Value = "calendar")] Calendar,
        [EnumMember(Value = "dropdown")] Dropdown,
        [EnumMember(Value = "multiselect")] Multiselect
    }
}
