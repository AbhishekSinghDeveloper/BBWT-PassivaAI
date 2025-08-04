using BBF.Reporting.Widget.Html.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.Html.Interfaces;

public interface IWidgetHtmlBuilderService : IEntityCreate<HtmlDTO>, IEntityUpdate<HtmlDTO>
{
    Task<HtmlDTO> Create(HtmlDTO build, string? userId, CancellationToken ct = default);

    Task<HtmlDTO> CreateDraft(HtmlDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}