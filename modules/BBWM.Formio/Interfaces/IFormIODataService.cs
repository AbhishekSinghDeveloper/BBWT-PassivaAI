using BBWM.Core.Services;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIODataService :
        IEntityUpdate<FormDataDTO>,
        IEntityDelete<int>,
        IEntityPage<FormDataPageDTO>
    {
        Task<bool> FormHasData(int revisionId, CancellationToken ct);
        Task<bool> SaveFormData(FormDataDTO dto, CancellationToken ct);
        Task<string> GetFormDataJson(int id, CancellationToken ct);
        Task<FormDataDraftDTO> SaveFormDataDraft(FormDataDraftDTO formDataDraft, CancellationToken ct);
        Task<FormDataDraftDTO> GetFormDataDraft(int id, string userId, CancellationToken ct);
        Task<bool> DiscardDraft(int id, bool clearUploadedFiles, CancellationToken ct);
        List<string> GetVersionsForFiltering(List<int> organizationIDs, bool isAdmin, CancellationToken ct);
        Task DeleteMultiple(List<int> idsToDelete, CancellationToken ct = default);
    }
}