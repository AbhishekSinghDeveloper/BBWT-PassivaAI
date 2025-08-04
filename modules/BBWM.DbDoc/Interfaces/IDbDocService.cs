using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DTO;

namespace BBWM.DbDoc.Interfaces;

public interface IDbDocService
{
    /// <summary>
    /// Determines whether a column metadata exists.
    /// </summary>
    /// <param name="id">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> ColumnMetadataExists(int id, CancellationToken ct = default);

    /// <summary>
    /// Copies a table metadata to a folder.
    /// </summary>
    /// <param name="dto">Copying info.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created table metadata.</returns>
    Task<FolderDTO> CopyTableMetadataToFolder(CopyTableMetadataToFolderDTO dto, CancellationToken ct = default);

    /// <summary>
    /// Deletes a table metadata.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FolderDTO> DeleteTableMetadata(int id, CancellationToken ct = default);

    /// <summary>
    /// Deletes a validation metadata from a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteColumnValidationMetadata(int columnMetadataId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a view metadata from a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteColumnViewMetadata(int columnMetadataId, CancellationToken ct = default);

    /// <summary>
    /// Searches a column metadata by the unique column ID in a specified folder. If the folder is not specified, then searching is carried out in the default folder.
    /// </summary>
    /// <param name="folderId">The ID of the folder where to search in.</param>
    /// <param name="columnUid">The unique column metadata ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ColumnMetadataDTO> GetColumnMetadata(Guid folderId, string columnUid, CancellationToken ct = default);

    /// <summary>
    /// Searches a table metadata by ID.
    /// </summary>
    /// <param name="id">ID of a searched table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<TableMetadataDTO> GetTableMetadata(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets table validation and view settings.
    /// </summary>
    /// <param name="uniqueTableId">The unique table ID.</param>
    /// <param name="containingFolderId">A folder that contains metadata. The default folder uses if not specified.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary, where key corresponds to table column name, and the value contains validation and view settings.</returns>
    Task<IDictionary<string, ColumnMetadataResult>> GetActualTableMetadata(string uniqueTableId, Guid? containingFolderId = null, CancellationToken ct = default);

    /// <summary>
    /// Gets table validation and view settings according to a specified table metadata.
    /// </summary>
    /// <param name="tableMetadataId">ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary, where key corresponds to table column name, and the value contains validation and view settings.</returns>
    Task<IDictionary<string, ColumnMetadataResult>> GetActualTableMetadata(int tableMetadataId, CancellationToken ct = default);

    /// <summary>
    /// Gets all actual validation rules for a specified model type in a specified folder. 
    /// </summary>
    /// <param name="modelType">The type of model.</param>
    /// <param name="sourceFolderId">Folder's ID where to search rules in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary where a key corresponds to a model property name and value to a rules list.</returns>
    Task<IDictionary<string, IEnumerable<ValidationRule>>> GetValidationRulesForModel(Type modelType, Guid sourceFolderId, CancellationToken ct = default);

    /// <summary>
    /// Copies a column type metadata to a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">Column metadata ID.</param>
    /// <param name="columnTypeId">Cpying column type ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Refreshed column metadata item.</returns>
    Task<ColumnMetadataDTO> SetColumnTypeMetadataForColumnMetadata(int columnMetadataId, Guid columnTypeId, CancellationToken ct = default);

    /// <summary>
    /// Creates a validation metadata for a column metadata. Replaces the existing.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="validationMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created validation metadata.</returns>
    Task<ColumnValidationMetadataDTO> SetValidationMetadata(int columnMetadataId, ColumnValidationMetadataDTO validationMetadata, CancellationToken ct = default);

    /// <summary>
    /// Creates a view metadata for a column metadata. Replaces the existing.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="viewMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created view metadata.</returns>
    Task<ColumnViewMetadataDTO> SetViewMetadata(int columnMetadataId, ColumnViewMetadataDTO viewMetadata, CancellationToken ct = default);

    /// <summary>
    /// Determines whether a table metadata exists.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> TableMetadataExists(int id, CancellationToken ct = default);

    /// <summary>
    /// Updates a column metadata.
    /// </summary>
    /// <param name="id">The ID of a column metadata.</param>
    /// <param name="columnMetadataDto">Data of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated column metadata.</returns>
    Task<ColumnMetadataDTO> UpdateColumnMetadata(int id, ColumnMetadataDTO columnMetadataDto, CancellationToken ct = default);

    /// <summary>
    /// Updates a table metadata.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="table">Data of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated table metadata.</returns>
    Task<TableMetadataDTO> UpdateTableMetadata(int id, TableMetadataDTO table, CancellationToken ct = default);

    Task<IEnumerable<ColumnMetadataDTO>> GetTableColumns(string folderId, string tableId, CancellationToken ct);
}
