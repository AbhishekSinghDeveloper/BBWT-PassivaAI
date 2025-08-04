using System.Runtime.Serialization;
using BBWM.Core.Web.JsonConverters;
using Newtonsoft.Json;

namespace BBF.Reporting.Core.Enums;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum QueryFilterMode
{
    [EnumMember(Value = "userOrganizationFilter")]
    UserOrganizationFilter,

    [EnumMember(Value = "userOrganizationsFilter")]
    UserOrganizationsFilter
}