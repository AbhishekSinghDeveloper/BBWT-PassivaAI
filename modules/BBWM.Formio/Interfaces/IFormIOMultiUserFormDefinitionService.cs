using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOMultiUserFormDefinitionService :
        IEntityQuery<MultiUserFormDefinition>,
        IEntityDelete<int>,
        IEntityPage<MultiUserFormDefinitionDTO>
    {
        Task<List<FormDefinitionDTO>> GetFormDefinitions(CancellationToken cancellationToken);
        Task<bool> NewMultiUserForm(NewMultiUserFormDefinitionDTO dto, CancellationToken cancellationToken);
        Task<List<MultiUserFormTargetDTO>> GetPossibleTargets(CancellationToken cancellationToken);

        Task<List<MultiUserFormTargetDTO>> GetInstanceTargets(int id, CancellationToken cancellationToken);
        Task<bool> IsMUFReady(int id, CancellationToken cancellationToken);
    }
}