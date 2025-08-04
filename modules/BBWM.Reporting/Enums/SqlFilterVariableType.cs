using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum SqlFilterVariableType
    {
        [EnumMember(Value = "tableColumn")] TableColumn,
        [EnumMember(Value = "filterControl")] FilterControl,
        [EnumMember(Value = "unknown")] Unknown
    }
}
