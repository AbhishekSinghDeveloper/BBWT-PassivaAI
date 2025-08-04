using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BBWM.Core.Web.JsonConverters;

namespace BBWM.FormIO.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum FormFieldChangeAction
{
    [EnumMember(Value = "add")] Add,
    [EnumMember(Value = "edit")] Edit,
    [EnumMember(Value = "remove")] Remove
}