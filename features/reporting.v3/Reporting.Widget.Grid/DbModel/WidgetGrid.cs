using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Enums;
using BBWM.Core.Data;

namespace BBF.Reporting.Widget.Grid.DbModel;

public class WidgetGrid : IAuditableEntity, IWidgetEntity
{
    public int Id { get; set; }

    public bool IsRowSelectable { get; set; }
    public bool SummaryFooterVisible { get; set; }
    public SortOrder? DefaultSortOrder { get; set; }
    public string? DefaultSortColumnAlias { get; set; }
    public bool ShowVisibleColumnsSelector { get; set; }

    // Foreign keys and navigational properties.
    public Guid? QuerySourceId { get; set; }
    public Guid WidgetSourceId { get; set; }

    public QuerySource? QuerySource { get; set; }
    public WidgetSource WidgetSource { get; set; } = null!;

    public IList<WidgetGridColumn> Columns { get; set; } = new List<WidgetGridColumn>();
}