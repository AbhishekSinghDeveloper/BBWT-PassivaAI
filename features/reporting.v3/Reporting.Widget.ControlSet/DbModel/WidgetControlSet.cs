using BBF.Reporting.Core.DbModel;
using BBWM.Core.Data;

namespace BBF.Reporting.Widget.ControlSet.DbModel;

public class WidgetControlSet : IAuditableEntity, IWidgetEntity
{
    public int Id { get; set; }

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }

    public WidgetSource WidgetSource { get; set; } = null!;

    public IList<WidgetControlSetItem> Items { get; set; } = new List<WidgetControlSetItem>();
}