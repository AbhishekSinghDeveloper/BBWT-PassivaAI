using BBWM.Core.Services;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOMultiUserFormStageService:
        IEntityPage<MultiUserFormStageDTO>
    {
        Task<List<MultiUserFormTargetDTO>> GetPossibleTargets(CancellationToken cancellationToken);
        Task<bool> UpdateMultiUserStage(MultiUserFormStageUpdateDTO dto, CancellationToken cancellationToken);

    }
}
