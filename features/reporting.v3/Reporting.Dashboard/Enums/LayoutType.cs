using System.Runtime.Serialization;
using BBWM.Core.Web.JsonConverters;
using Newtonsoft.Json;

namespace BBF.Reporting.Dashboard.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum LayoutType
{
    [EnumMember(Value = "cards")] Cards,
    [EnumMember(Value = "dividers")] Dividers,
    [EnumMember(Value = "plain")] Plain
}