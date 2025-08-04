using BBF.Reporting.Core.Enums;
using BBF.Reporting.Widget.ControlSet.Enums;
using BBWM.Core.DTO;
using System.Text.Json.Nodes;
using BBF.Reporting.Core.DTO;

namespace BBF.Reporting.Widget.ControlSet.DTO;

public class ControlSetViewItemDTO : IDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public DataType? DataType { get; set; }
    public InputType InputType { get; set; }

    public string HintText { get; set; } = null!;
    public JsonNode ExtraSettings { get; set; } = null!;
    public bool EmptyFilterIfFalse { get; set; }
    public bool UserCanChangeOperator { get; set; }
    public ControlValueEmitType ValueEmitType { get; set; }

    public string? TableId { get; set; }
    public string? ParentTableId { get; set; }
    public string? FolderId { get; set; }
    public string? SourceCode { get; set; }
    public string? ValueColumnId { get; set; }
    public string? LabelColumnId { get; set; }
    public string? VariableName { get; set; }

    // Foreign keys and navigational properties.
    public int ControlSetId { get; set; }
    public int? FilterRuleId { get; set; }

    public FilterRuleDTO? FilterRule { get; set; }
}