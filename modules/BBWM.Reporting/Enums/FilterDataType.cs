using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum FilterDataType
    {
        [EnumMember(Value = "text")] Text = 0,
        [EnumMember(Value = "numeric")] Numeric = 1,
        [EnumMember(Value = "date")] Date = 2,
        [EnumMember(Value = "boolean")] Boolean = 3
    }
}
