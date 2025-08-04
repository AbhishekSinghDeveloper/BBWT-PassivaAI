using BBF.Reporting.Core.Interfaces;

namespace BBF.Reporting.Widget.Chart.Interfaces;

public interface IWidgetChartProvider : IWidgetSourceProvider
{
    Task<Guid> ReleaseQueryDraft(Guid querySourceId, CancellationToken ct = default);
}