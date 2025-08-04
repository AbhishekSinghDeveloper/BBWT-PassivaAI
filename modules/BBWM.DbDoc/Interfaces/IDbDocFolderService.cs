using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;

namespace BBWM.DbDoc.Interfaces;

public interface IDbDocFolderService
{
    /// <summary>
    /// Gets default folder name. Default folder is created by DB context models defined in code.
    /// </summary>
    string DefaultFolderName { get; }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="folder">Data of a new folder.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created folder.</returns>
    Task<FolderDTO> CreateFolder(FolderDTO folder, CancellationToken ct = default);

    /// <summary>
    /// Deletes a folder.
    /// </summary>
    /// <param name="folderId">The ID of a folder.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<FolderDTO>> DeleteFolder(Guid folderId, CancellationToken ct = default);

    /// <summary>
    /// Determines whether a folder exists.
    /// </summary>
    /// <param name="folderId">The ID of a folder.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> FolderExists(Guid folderId, CancellationToken ct = default);


    Task AddMainDbFolderOwner(string owner, CancellationToken ct = default);

    /// <summary>
    /// Adds owner to a folder's owners list
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task AddFolderOwner(Guid folderId, string owner, CancellationToken ct = default);

    /// <summary>
    /// Gets folders filtered by owner excluding tables list.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<FolderDTO>> GetOwnerFolders(string owner, CancellationToken ct = default);

    /// <summary>
    /// Gets table metadata list filtered by a parent folder.
    /// </summary>
    /// <param name="folderId">Parent folder ID.</param>
    Task<IEnumerable<TableMetadataDTO>> GetFolderTableMatadata(Guid folderId, CancellationToken ct = default);

    Task<IEnumerable<TableMetadataDTO>> GetFullTablesMatadata(Guid folderId, IEnumerable<string> tableMetadataIds, CancellationToken ct = default);

    /// <summary>
    /// Gets folders excluding tables list.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<FolderDTO>> GetDbExplorerFolders(CancellationToken ct = default);

    /// <summary>
    /// Returns default folder with all tables existing in DB.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<FolderDTO> GetDefaultFolder(CancellationToken ct = default);

    /// <summary>
    /// Searches a folder by ID.
    /// </summary>
    /// <param name="folderId">ID of a searched folder.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FolderDTO> GetFolder(Guid folderId, CancellationToken ct = default);

    /// <summary>
    /// Gets folder's DB source by folder ID.
    /// </summary>
    /// <param name="folderId">ID of the folder.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Guid?> GetFolderDatabaseSourceId(Guid folderId, CancellationToken ct = default);

    /// <summary>
    /// Updates a folder.
    /// </summary>
    /// <param name="folderId">The ID of a folder.</param>
    /// <param name="folder">Data of a folder.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated folder.</returns>
    Task<FolderDTO> UpdateFolder(Guid folderId, FolderDTO folder, CancellationToken ct = default);

    /// <summary>
    /// Gets all DB path macros for all default aliases
    /// </summary>
    /// <param name="folderId">ID of folder which database source's DB schema is used for macros searching</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<DbPathMacroDTO>> GetDbPathMacrosAllAliases(Guid folderId, CancellationToken ct = default);

    Task RemoveFolderTables(Guid folderId, CancellationToken ct);
}
