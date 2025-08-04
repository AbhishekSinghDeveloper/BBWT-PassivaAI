using BBF.Reporting.Core.DbModel;

namespace BBF.Reporting.Core.Interfaces;

public interface IWidgetSourceProvider
{
    Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default);

    Task<IEnumerable<WidgetSource>> GetAttachedWidgets(Guid querySourceId, CancellationToken ct = default);

    Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default);
}