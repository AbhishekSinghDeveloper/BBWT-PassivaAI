using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BBWM.Core.Web.JsonConverters;

namespace BBF.Reporting.Widget.Chart.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum ColumnPurpose
{
    [EnumMember(Value = "axisX")] AxisX,
    [EnumMember(Value = "axisY")] AxisY,
    [EnumMember(Value = "series")] Series,
    [EnumMember(Value = "bubbleSize")] BubbleSize,
    [EnumMember(Value = "tooltip")] Tooltip,
}