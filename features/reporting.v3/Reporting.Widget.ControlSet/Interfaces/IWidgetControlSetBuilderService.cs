using BBF.Reporting.Widget.ControlSet.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.ControlSet.Interfaces;

public interface IWidgetControlSetBuilderService : IEntityCreate<ControlSetViewDTO>, IEntityUpdate<ControlSetViewDTO>
{
    Task<ControlSetViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);

    Task<ControlSetViewDTO> Create(ControlSetViewDTO build, string? userId, CancellationToken ct = default);

    Task<ControlSetViewDTO> CreateDraft(ControlSetViewDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}