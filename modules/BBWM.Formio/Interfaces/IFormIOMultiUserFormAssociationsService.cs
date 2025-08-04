using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOMultiUserFormAssociationsService :
        IEntityQuery<MultiUserFormAssociations>,
        IEntityDelete<int>,
        IEntityPage<MultiUserFormAssociationsDTO>
    {
        Task<bool> NewMultiUserFormAssociation(NewMultiUserFormAssociationsDTO dto, CancellationToken cancellationToken);
        Task<MultiUserFormAssociationsDTO> GetMUFDataForRendering(int id, string targetUserId, CancellationToken cancellationToken);
    }
}
