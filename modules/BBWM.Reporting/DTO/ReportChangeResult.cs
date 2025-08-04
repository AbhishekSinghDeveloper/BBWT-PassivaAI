using BBWM.Core.Web.JsonConverters;
using BBWM.Reporting.Model;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BBWM.Reporting.DTO;

[JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
public enum ReportChangeType
{
    [EnumMember(Value = "created")] Created,
    [EnumMember(Value = "modified")] Modified,
    [EnumMember(Value = "deleted")] Deleted
}

public class ReportChangeResult
{
    public DateTimeOffset ReportUpdatedOn { get; set; }

    public dynamic RequestTargetPart { get; set; }

    public IList<ReportAdditionalChangedPart> AdditionalChangedParts { get; set; } = new List<ReportAdditionalChangedPart>();
}

public class ReportAdditionalChangedPart
{
    public const string FilterControlAdditionalChangedPartName = nameof(FilterControl);
    public const string GridViewColumnAdditionalChangedPartName = nameof(GridViewColumn);
    public const string SectionAdditionalChangedPartName = nameof(Section);
    public const string QueryFilterBindingAdditionalChangedPartName = nameof(QueryFilterBinding);
    public const string QueryFilterAdditionalChangedPartName = nameof(QueryFilter);
    public const string QueryTableJoinAdditionalChangedPartName = nameof(QueryTableJoin);


    public dynamic ChangedPartData { get; set; }

    public dynamic ChangedPartId { get; set; }

    public string ChangedPartName { get; set; } = string.Empty;

    public ReportChangeType ChangedPartType { get; set; }
}
