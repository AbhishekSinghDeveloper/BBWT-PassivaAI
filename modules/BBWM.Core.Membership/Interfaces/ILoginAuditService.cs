using BBWM.Core.Membership.Model;

namespace BBWM.Core.Membership.Interfaces;

public interface ILoginAuditService
{
    Task<int> GetLastAttemptsCountAsync(string ip, DateTimeOffset withInDate);
    public Task<LoginAudit> GetLastSuccessfulLoginAuditAsync(string userEmail);
    public Task<LoginAudit> GetLastSignedOutAuditAsync(string userEmail);
    public Task<LoginAudit> GetLastPassed2FACodeAuditAsync(string userEmail);
    public Task SaveLoginAuditAsync(User user, string result);
}
