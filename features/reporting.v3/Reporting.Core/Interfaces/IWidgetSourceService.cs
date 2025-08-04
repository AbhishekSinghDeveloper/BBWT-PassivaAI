using BBF.Reporting.Core.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Core.Interfaces;

public interface IWidgetSourceService :
    IEntityUpdate<WidgetSourceDTO>,
    IEntityDelete<Guid>,
    IEntityCreate<WidgetSourceDTO>
{
    Task<WidgetSourceDTO> Create(WidgetSourceDTO dto, string widgetType, string? ownerId = null,
        CancellationToken ct = default);

    Task<WidgetSourceDTO> CreateDraft(WidgetSourceDTO dto, string widgetType, Guid? releaseWidgetId = null,
        CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}