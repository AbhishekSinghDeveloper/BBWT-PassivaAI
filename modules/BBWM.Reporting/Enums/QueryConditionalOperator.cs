using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum QueryConditionalOperator
    {
        [EnumMember(Value = "and")] And,
        [EnumMember(Value = "or")] Or
    }
}
