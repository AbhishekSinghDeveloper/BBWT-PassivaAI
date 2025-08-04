using BBF.Reporting.Core.DTO;
using BBWM.Core.DTO;

namespace BBF.Reporting.Widget.Html.DTO;

public class HtmlViewDTO : IDTO
{
    public int Id { get; set; }
    public string InnerHtml { get; set; } = null!;

    // Foreign keys and navigational properties.
    public Guid WidgetSourceId { get; set; }

    public WidgetSourceDTO WidgetSource { get; set; } = null!;
}