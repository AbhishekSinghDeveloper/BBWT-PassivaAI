using BBF.Reporting.Core.DbModel;
using BBWM.Core.Data;

namespace BBF.Reporting.Widget.Html.DbModel;

public class WidgetHtml : IAuditableEntity, IWidgetEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Inner HTML string of the component.
    /// </summary>
    public string InnerHtml { get; set; } = null!;

    // Foreign key and navigational properties.
    public Guid WidgetSourceId { get; set; }

    public WidgetSource WidgetSource { get; set; } = null!;
}