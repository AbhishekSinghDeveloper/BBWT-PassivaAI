using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums
{
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum QueryFilterBindingType
    {
        [EnumMember(Value = "filterControl")] FilterControl = 0,
        [EnumMember(Value = "masterDetailGrid")] MasterDetailGrid = 1,
    }
}
