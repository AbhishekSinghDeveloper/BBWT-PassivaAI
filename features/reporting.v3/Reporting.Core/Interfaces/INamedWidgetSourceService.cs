using BBF.Reporting.Core.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Core.Interfaces;

public interface INamedWidgetSourceService :
    IEntityPage<WidgetSourceDTO>
{
    Task<WidgetSourcePreloadDTO> GetByCode(string widgetCode, CancellationToken ct = default);

    Task<IEnumerable<WidgetSourceDTO>> GetAll(CancellationToken ct = default);

    Task ChangeOwner(Guid widgetSourceId, string userId, CancellationToken ct = default);

    Task PublishWidget(Guid widgetSourceId, IEnumerable<int> organizationIds, CancellationToken ct = default);

    Task<bool> UserHasAccessToWidgetSource(Guid widgetSourceId, CancellationToken ct = default);

    Task<bool> UserHasAccessToWidgetSource(string widgetCode, CancellationToken ct = default);
}