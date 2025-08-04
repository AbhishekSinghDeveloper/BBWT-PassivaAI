using BBWM.Core.Services;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIOCategoryService :
        IEntityCreate<FormCategoryDTO>,
        IEntityDelete<int>
    {
        Task<List<FormCategoryDTO>> GetAll(CancellationToken ct);
    }
}