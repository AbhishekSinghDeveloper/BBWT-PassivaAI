using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BBWM.Core.Web.JsonConverters;

namespace BBF.Reporting.Core.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum EmittedVariableBehavior
{
    [EnumMember(Value = "populate")] Populate = 0,
    [EnumMember(Value = "clean")] Clean = 1,
}