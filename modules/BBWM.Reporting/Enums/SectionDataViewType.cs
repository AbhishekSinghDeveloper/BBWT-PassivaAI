using BBWM.Core.Web.JsonConverters;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum SectionDataViewType
{
    [EnumMember(Value = "dataGrid")] DataGrid = 0,
    [EnumMember(Value = "noView")] NoView = 1
}
