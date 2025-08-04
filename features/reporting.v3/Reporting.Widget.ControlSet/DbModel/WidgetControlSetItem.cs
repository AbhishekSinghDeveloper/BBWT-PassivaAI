using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Widget.ControlSet.Enums;
using BBWM.Core;
using BBWM.Core.Data;

namespace BBF.Reporting.Widget.ControlSet.DbModel;

public class WidgetControlSetItem : IAuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public DataType? DataType { get; set; }
    public InputType InputType { get; set; }

    public string HintText { get; set; } = null!;
    public string ExtraSettings { get; set; } = null!;
    public bool EmptyFilterIfFalse { get; set; }
    public bool UserCanChangeOperator { get; set; }
    public ControlValueEmitType ValueEmitType { get; set; }

    public string? TableId { get; set; }
    public string? ParentTableId { get; set; }
    public string? FolderId { get; set; }
    public string? SourceCode { get; set; }
    public string? ValueColumnId { get; set; }
    public string? LabelColumnId { get; set; }

    // Foreign keys and navigational properties.
    public int? VariableId { get; set; }
    public int ControlSetId { get; set; }
    public int? FilterRuleId { get; set; }

    public Variable? Variable { get; set; }
    public WidgetControlSet ControlSet { get; set; } = null!;
    [DoNotAutoignore] public FilterRule? FilterRule { get; set; }
}