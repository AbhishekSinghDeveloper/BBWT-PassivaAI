using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

namespace BBWM.Core.Membership.Interfaces;

public interface IBrandingService :
    IEntityQuery<Branding>
{
    Task DeleteLogoIcon(int brandingId, CancellationToken cancellationToken = default);
    Task DeleteLogoImage(int brandingId, CancellationToken cancellationToken = default);
    Task<BrandingDTO> GetOrganizationBranding(int organizationId, CancellationToken cancellationToken = default);
}
