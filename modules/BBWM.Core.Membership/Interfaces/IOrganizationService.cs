using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;

namespace BBWM.Core.Membership.Interfaces;

public interface IOrganizationService :
    IEntityQuery<Organization>,
    IEntityDelete<int>
{
    Task<OrganizationDTO> Get(string name, int skipId, CancellationToken ct);
}
