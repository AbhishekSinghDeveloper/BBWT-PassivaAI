using BBF.Reporting.Core.Interfaces;

namespace BBF.Reporting.Widget.Grid.Interfaces;

public interface IWidgetGridProvider : IWidgetSourceProvider
{
    Task<Guid> ReleaseQueryDraft(Guid querySourceId, CancellationToken ct = default);
}