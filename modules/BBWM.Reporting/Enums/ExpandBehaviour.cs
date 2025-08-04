using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum ExpandBehaviour
    {
        [EnumMember(Value = "noContainer")] NoContainer = 0,
        [EnumMember(Value = "Static")] Static = 1,
        [EnumMember(Value = "InitiallyExpanded")] InitiallyExpanded = 2,
        [EnumMember(Value = "InitiallyCollapsed")] InitiallyCollapsed = 3
    }
}
