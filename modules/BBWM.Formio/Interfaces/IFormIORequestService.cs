using BBWM.Core.Services;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIORequestService :
        IEntityQuery<FormRequest>,
        IEntityPage<FormRequestPageDTO>
    {
        Task<FormRequestTargetsDTO> GetTargets(CancellationToken cancellationToken);
        Task<bool> CreateNewRequest(FormRequestDTO dto, CancellationToken ct = default);
    }
}
