using BBWM.Core.Services;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;

namespace BBWM.DbDoc.Interfaces;

public interface IColumnTypeService :
    IEntityQuery<ColumnType>,
    IEntityCreate<ColumnTypeDTO>,
    IEntityUpdate<ColumnTypeDTO>,
    IEntityDelete<Guid>
{
    /// <summary>
    /// Gets all entities in a table.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<ColumnTypeDTO>> GetAll(CancellationToken ct);

    /// <summary>
    /// Deletes a validation metadata from a column type.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteValidationMetadata(Guid columnTypeId, CancellationToken ct);

    /// <summary>
    /// Deletes a view metadata from a column type.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteViewMetadata(Guid columnTypeId, CancellationToken ct);

    /// <summary>
    /// Creates validation metadata for a column type. Replaces the existing.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="validationMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created validation metadata.</returns>
    Task<ColumnValidationMetadataDTO> SetValidationMetadata(Guid columnTypeId, ColumnValidationMetadataDTO validationMetadata, CancellationToken ct);

    /// <summary>
    /// Creates view metadata for a column type. Replaces the existing.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="viewMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created view metadata.</returns>
    Task<ColumnViewMetadataDTO> SetViewMetadata(Guid columnTypeId, ColumnViewMetadataDTO viewMetadata, CancellationToken ct);
}