using BBWM.Core.Web.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBF.Reporting.Widget.ControlSet.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum ControlValueEmitType
{
    /// <summary>
    /// Control's value emitting is triggered on the control change event only,
    /// disregarding the controls group submitting (Search button).
    /// Control's changing only emit a variable of the control.
    /// </summary>
    [EnumMember(Value = "standalone")] Standalone = 0,

    /// <summary>
    /// Control's value emitting is triggered on the controls group submitting (Search button) only,
    /// disregarding the control change event
    /// </summary>
    [EnumMember(Value = "grouped")] Grouped = 1,

    /// <summary>
    /// Control's value emitting is triggered on the controls group submitting (Search button)
    /// or on the control change event.
    /// In case of control's changing, all variables of group are emitted.
    /// </summary>
    [EnumMember(Value = "singleAndGrouped")] SingleAndGrouped = 2
}