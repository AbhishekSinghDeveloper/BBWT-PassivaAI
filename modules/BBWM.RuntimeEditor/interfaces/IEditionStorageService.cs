namespace BBWM.RuntimeEditor.interfaces;

public interface IEditionStorageService
{
    Task<RteDictionary> GetDictionary(CancellationToken ct);
    Task<RteEdition> GetEdition(CancellationToken ct);
    Task<RteEditionUpdate> SaveEdition(RteEdition edition, string editorUserId, CancellationToken ct);
}
