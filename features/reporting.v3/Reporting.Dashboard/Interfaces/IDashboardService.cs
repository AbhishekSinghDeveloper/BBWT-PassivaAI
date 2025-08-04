using BBF.Reporting.Dashboard.DTO;
using BBWM.Core.Services;

namespace BBF.Reporting.Dashboard.Interfaces;

public interface IDashboardService : IEntityPage<DashboardDTO>, IEntityDelete<Guid>
{
    Task ChangeOwner(Guid dashboardId, string userId, CancellationToken ct = default);
    Task PublishDashboard(Guid dashboardId, IEnumerable<int> organizationIds, CancellationToken ct = default);
    Task<bool> UserHasAccessToDashboard(Guid dashboardId, CancellationToken ct = default);
    Task<bool> UserHasAccessToDashboard(string urlSlug, CancellationToken ct = default);
}