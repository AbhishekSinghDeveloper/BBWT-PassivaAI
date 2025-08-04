using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;

namespace BBWM.Core.Membership.Interfaces;

public interface IUser2FAService
{
    Task<Enabling2FADTO> Get2FAEnablingData(string userId, CancellationToken cancellationToken = default);

    Task Enable2FA(Enabling2FADTO dto, string userId, CancellationToken cancellationToken = default);

    Task Disable2FA(string userId, string code, CancellationToken cancellationToken = default);

    bool IsUserRequiredSetup2FA(User user);

    Task Verify2FACode(Checking2FADTO dto, CancellationToken cancellationToken = default);
}
