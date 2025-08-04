using BBF.Reporting.Widget.Grid.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.Grid.Interfaces;

public interface IWidgetGridBuilderService : IEntityCreate<GridViewDTO>, IEntityUpdate<GridViewDTO>
{
    Task<GridViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default);

    Task<GridViewDTO> Create(GridViewDTO build, string? userId, CancellationToken ct = default);

    Task<GridViewDTO> CreateDraft(GridViewDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}