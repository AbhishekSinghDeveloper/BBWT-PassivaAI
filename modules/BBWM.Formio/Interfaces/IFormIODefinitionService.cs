using BBWM.Core.Services;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Interfaces
{
    public interface IFormIODefinitionService :
        IEntityGet<FormDefinitionDTO, int>,
        IEntityUpdate<FormDefinitionDTO>,
        IEntityDelete<int>,
        IEntityPage<FormDefinitionPageDTO>
    {
        Task<FormDefinitionComposedDTO> GetFormDefinitionJson(int? definitionId, int? revisionId, bool readOnly, List<string> paramList,
            CancellationToken cancellationToken);

        Task<bool> PublishFormDefinition(PublishFormDefinitionDTO publishFormDefinitionDTO, CancellationToken cancellationToken);
        Task<FormDefinitionDTO> Create(FormDefinitionForNewRequestDTO formDefinition, CancellationToken cancellationToken);
        Task<int> Copy(FormDefinitionDTO formDefinition, CancellationToken cancellationToken);
        List<string> GetAvailableVersionsForFiltering(List<int> orgIds, bool isAdmin, string userId, CancellationToken cancellationToken);
        Task<bool> ChangeFormDesignOwnership(ChangeFormDefinitionOwnerDTO changeFormDefinitionOwner, CancellationToken cancellationToken);
    }
}