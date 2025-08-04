using BBWM.Core.Services;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Interfaces;

public interface IFormioIORevisionService
    : IEntityPage<FormRevisionDTO>,
    IEntityDelete<int>
{
    Task<FormRevisionDTO> Create(NewFormRevisionRequestDTO dto, CancellationToken ct);
    Task<bool> Update(int formRevisionId, UpdateFormRevisionRequestDTO dto, CancellationToken ct = default);

    Task<bool> SetActive(int formId, int revisionId, CancellationToken ct);

    Task<FormRevisionDTO> CreateInitialFormRevision(int formDefinitionId, InitialFormRevisionRequestDTO dto, CancellationToken ct);
}
