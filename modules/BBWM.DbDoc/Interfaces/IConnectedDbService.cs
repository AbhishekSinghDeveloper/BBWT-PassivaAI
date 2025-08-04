using BBWM.DbDoc.DTO;

namespace BBWM.DbDoc.Interfaces;

public interface IConnectedDbService
{
    /// <summary>
    /// Creates DB DOC folder by a given connections string of an external DB source.
    /// DB source's DB schema is converted to the folder tables structure
    /// </summary>
    Task<Guid> CreateFolderByDbConnection(CreateFolderByDbConnectionRequest addRequest, CancellationToken ct);

    // TODO: return folder ID instead
    /// <summary>
    /// Refreshes DB DOC folder's tables structure from an external DB source that the folder is connected to.
    /// </summary>
    Task<FolderDTO> SyncFolderFromDatabaseSource(Guid folderId, CancellationToken ct);
}