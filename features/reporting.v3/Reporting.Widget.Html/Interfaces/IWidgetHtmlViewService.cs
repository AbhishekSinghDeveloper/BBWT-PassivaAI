using BBF.Reporting.Widget.Html.DTO;

namespace BBF.Reporting.Widget.Html.Interfaces;

public interface IWidgetHtmlViewService
{
    Task<HtmlViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);
}