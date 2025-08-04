using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Extensions;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;

namespace BBWM.DbDoc.Services;

public class DbDocService : IDbDocService
{
    private readonly IDbContext _context;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IDbSchemaManager _dbSchemaManager;
    private readonly IDbDocGitLabService _dbDocGitLabService;
    private readonly IMapper _mapper;

    public DbDocService(
        IDbContext context,
        IDbDocFolderService dbDocFolderService,
        IDbSchemaManager dbSchemaManager,
        IDbDocGitLabService dbDocGitLabService,
        IMapper mapper)
    {
        _context = context;
        _dbDocFolderService = dbDocFolderService;
        _dbSchemaManager = dbSchemaManager;
        _dbDocGitLabService = dbDocGitLabService;
        _mapper = mapper;
    }

    /// <summary>
    /// Determines whether a column metadata exists.
    /// </summary>
    /// <param name="id">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<bool> ColumnMetadataExists(int id, CancellationToken ct = default) =>
        await _context.Set<ColumnMetadata>().AnyAsync(x => x.Id == id, ct);

    /// <summary>
    /// Copies a table metadata to a folder.
    /// </summary>
    /// <param name="dto">Copying info.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created table metadata.</returns>
    public async Task<FolderDTO> CopyTableMetadataToFolder(CopyTableMetadataToFolderDTO dto, CancellationToken ct = default)
    {
        var folderCopyTo = await _context.Set<Folder>().FirstOrDefaultAsync(x => x.Id == dto.FolderIdCopyTo, ct)
            ?? throw new ObjectNotExistsException($"The folder with ID '{dto.FolderIdCopyTo}' you're trying to add the table to doesn't exist.");

        var tableMetadata = await TablesDeepQueryable().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == dto.CopyingTableMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The copying table '{dto.CopyingTableMetadataId}' doesn't exist.");

        if (await _context.Set<TableMetadata>().AnyAsync(x => x.FolderId == dto.FolderIdCopyTo && x.TableId == tableMetadata.TableId, ct))
            throw new BusinessException($"The folder '{folderCopyTo.Name}' already contains table '{tableMetadata.TableId}'.");

        var tableMetadataCopy = tableMetadata.DeepCopy();
        tableMetadataCopy.Id = default;
        tableMetadataCopy.Folder = null;
        tableMetadataCopy.FolderId = dto.FolderIdCopyTo;

        foreach (var columnCopy in tableMetadataCopy.Columns)
        {
            columnCopy.Id = default;
            columnCopy.TableId = default;
            columnCopy.Table = null;

            if (columnCopy.ValidationMetadata != null)
            {
                columnCopy.ValidationMetadataId = null;
                columnCopy.ValidationMetadata.Id = default;
            }

            if (columnCopy.ViewMetadata != null)
            {
                columnCopy.ViewMetadataId = null;
                columnCopy.ViewMetadata.Id = default;

                if (columnCopy.ViewMetadata.GridColumnView != null)
                {
                    columnCopy.ViewMetadata.GridColumnView.Id = default;
                    columnCopy.ViewMetadata.GridColumnView.ColumnViewMetadataId = default;
                    columnCopy.ViewMetadata.GridColumnView.ColumnViewMetadata = null;
                }
            }

            //TODO: issue! On copying, this duplicates column type record. To fix!
            if (columnCopy.ColumnType != null)
            {
                columnCopy.ColumnTypeId = null;
                columnCopy.ColumnType.Id = default;

                if (columnCopy.ColumnType.ValidationMetadata != null)
                {
                    columnCopy.ColumnType.ValidationMetadataId = null;
                    columnCopy.ColumnType.ValidationMetadata.Id = default;
                }

                if (columnCopy.ColumnType.ViewMetadata != null)
                {
                    columnCopy.ColumnType.ViewMetadataId = null;
                    columnCopy.ColumnType.ViewMetadata.Id = default;

                    if (columnCopy.ColumnType.ViewMetadata.GridColumnView != null)
                    {
                        columnCopy.ColumnType.ViewMetadata.GridColumnView.Id = default;
                        columnCopy.ColumnType.ViewMetadata.GridColumnView.ColumnViewMetadataId = default;
                        columnCopy.ColumnType.ViewMetadata.GridColumnView.ColumnViewMetadata = null;
                    }
                }
            }
        }

        await _context.Set<TableMetadata>().AddAsync(tableMetadataCopy, ct);
        await _context.SaveChangesAsync(ct);

        await SetCurrentChangedOnTime(dto.FolderIdCopyTo, ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return await _dbDocFolderService.GetFolder(dto.FolderIdCopyTo, ct);
    }

    /// <summary>
    /// Deletes a validation metadata from a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteColumnValidationMetadata(int columnMetadataId, CancellationToken ct = default)
    {
        var column = await _context.Set<ColumnMetadata>()
            .Include(x => x.ValidationMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column type with ID '{columnMetadataId}' doesn't exist.");

        if (column.ValidationMetadata == null) return;

        await RemoveColumnValidationMetadataEntity(column, ct);
        await _context.SaveChangesAsync(ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);
    }

    /// <summary>
    /// Deletes a view metadata from a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteColumnViewMetadata(int columnMetadataId, CancellationToken ct = default)
    {
        var column = await _context.Set<ColumnMetadata>()
            .Include(x => x.ViewMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column type with ID '{columnMetadataId}' doesn't exist.");

        if (column.ViewMetadata == null) return;

        await RemoveColumnViewMetadataEntity(column, ct);
        await _context.SaveChangesAsync(ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);
    }

    /// <summary>
    /// Deletes a table metadata.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<FolderDTO> DeleteTableMetadata(int id, CancellationToken ct = default)
    {
        var table = await TablesDeepQueryable().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ObjectNotExistsException($"The table with ID '{id}' doesn't exist.");

        var folderId = table.FolderId;

        if ((await GetDefaultFolderEntity(ct)).Id == folderId)
            throw new BusinessException("The table can not be deleted from the default folder.");

        await RemoveTableMetadataEntity(table, ct);

        await SetCurrentChangedOnTime(folderId, ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return await _dbDocFolderService.GetFolder(folderId, ct);
    }

    public async Task<ColumnMetadataDTO> GetColumnMetadata(Guid folderId, string columnUid, CancellationToken ct = default)
    {
        var column = await ColumnsDeepQueryable()
            .SingleOrDefaultAsync(x => x.Table.FolderId == folderId && columnUid == x.ColumnId, ct);
        return _mapper.Map<ColumnMetadata, ColumnMetadataDTO>(column);
    }

    /// <summary>
    /// Searches a table metadata by ID.
    /// </summary>
    /// <param name="id">ID of a searched table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<TableMetadataDTO> GetTableMetadata(int id, CancellationToken ct = default) =>
        _mapper.Map<TableMetadata, TableMetadataDTO>(
            await TablesDeepQueryable().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct));

    /// <summary>
    /// Gets table validation and view settings.
    /// </summary>
    /// <param name="tableId">Table metadata ID.</param>
    /// <param name="folderId">A folder that contains metadata. The default folder uses if not specified.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary, where key corresponds to table column name, and the value contains validation and view settings.</returns>
    public async Task<IDictionary<string, ColumnMetadataResult>> GetActualTableMetadata(string tableId, Guid? containingFolderId = null, CancellationToken ct = default)
    {
        var folderId = containingFolderId;
        if (folderId == null)
        {
            folderId = (await GetDefaultFolderEntity(ct))?.Id;

            if (folderId == default)
                throw new ConflictException("The default folder is not found.");
        }
        else
        {
            if (!await _dbDocFolderService.FolderExists((Guid)folderId, ct))
                throw new ObjectNotExistsException($"The folder with ID '{folderId}' doesn't exist.");
        }

        var tableMetadata = _mapper.Map<TableMetadata, TableMetadataDTO>(await TablesDeepQueryable()
            .SingleOrDefaultAsync(x => x.TableId == tableId && x.FolderId == folderId, ct))
            ?? throw new BusinessException($"The folder with ID '{folderId}' doesn't contain table '{tableId}'.");

        return GetActualTableMetadata(tableMetadata);
    }

    /// <summary>
    /// Gets table validation and view settings according to a specified table metadata.
    /// </summary>
    /// <param name="tableMetadataId">ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary, where key corresponds to table column name, and the value contains validation and view settings.</returns>
    public async Task<IDictionary<string, ColumnMetadataResult>> GetActualTableMetadata(int tableMetadataId, CancellationToken ct = default)
    {
        var tableMetadata = _mapper.Map<TableMetadata, TableMetadataDTO>(await TablesDeepQueryable()
            .SingleOrDefaultAsync(x => x.Id == tableMetadataId, ct))
            ?? throw new ObjectNotExistsException($"The table metadata item with ID '{tableMetadataId}' doesn't exist.");

        return GetActualTableMetadata(tableMetadata);
    }

    /// <summary>
    /// Gets all actual validation rules for a specified model type in a specified folder. The default folder uses if not specified.
    /// </summary>
    /// <param name="modelType">The type of model.</param>
    /// <param name="sourceFolderId">Folder's ID where to search rules in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dictionary where a key corresponds to a model property name and value to a rules list.</returns>
    public async Task<IDictionary<string, IEnumerable<ValidationRule>>> GetValidationRulesForModel(
        Type modelType, Guid sourceFolderId, CancellationToken ct = default)
    {
        var folder = await _context.Set<Folder>().SingleOrDefaultAsync(x => x.Id == sourceFolderId, ct)
            ?? throw new ObjectNotExistsException($"The folder with ID '{sourceFolderId}' doesn't exist.");

        var tableUniqueId = _dbSchemaManager.GetDbSchema(folder.DatabaseSourceId.Value).Tables
                .SingleOrDefault(x => x.Value.TableName.ToLowerInvariant() == modelType.ToString().ToLowerInvariant()).Key
                ?? throw new ConflictException($"The model with type '{modelType}' doesn't exist.");

        var tableMetadata = _context.Set<TableMetadata>()
            .SingleOrDefault(x => x.TableId == tableUniqueId && x.Folder.Id == folder.Id)
            ?? throw new BusinessException($"The folder '{folder}' doesn't contain table '{tableUniqueId}'.");

        var result = new Dictionary<string, IEnumerable<ValidationRule>>();
        var tableMetadataDto = await GetTableMetadata(tableMetadata.Id, ct);

        foreach (var columnMetadata in tableMetadataDto.Columns)
        {
            if (columnMetadata.ColumnType?.ValidationMetadata?.Rules != null)
            {
                result.Add(columnMetadata.StaticData.PropertyName, columnMetadata.ColumnType.ValidationMetadata.Rules);
            }
            else
            {
                if (columnMetadata.ValidationMetadata?.Rules != null)
                {
                    result.Add(columnMetadata.StaticData.PropertyName, columnMetadata.ValidationMetadata.Rules);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Copies a column type metadata to a column metadata.
    /// </summary>
    /// <param name="columnMetadataId">Column metadata ID.</param>
    /// <param name="columnTypeId">Cpying column type ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Refreshed column metadata item.</returns>
    public async Task<ColumnMetadataDTO> SetColumnTypeMetadataForColumnMetadata(int columnMetadataId, Guid columnTypeId, CancellationToken ct = default)
    {
        var columnMetadata = await ColumnsDeepQueryable().SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column metadata item with ID '{columnMetadataId}' does not exist.");

        var columnType = await _context.Set<ColumnType>()
            .Include(x => x.ValidationMetadata)
            .Include(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == columnTypeId, ct)
            ?? throw new ObjectNotExistsException($"The column type with ID '{columnTypeId}' does not exist.");

        columnMetadata.AnonymizationRule = columnType.AnonymizationRule;
        columnMetadata.ColumnTypeId = null;

        if (columnType.ValidationMetadata is not null)
            await RemoveColumnValidationMetadataSet(columnMetadata, _mapper.Map<ColumnValidationMetadata, ColumnValidationMetadataDTO>(columnType.ValidationMetadata), ct);
        else
            await RemoveColumnValidationMetadataEntity(columnMetadata, ct);

        if (columnType.ViewMetadata is not null)
            await RemoveColumnViewMetadataSet(columnMetadata, _mapper.Map<ColumnViewMetadata, ColumnViewMetadataDTO>(columnType.ViewMetadata), ct);
        else
            await RemoveColumnViewMetadataEntity(columnMetadata, ct);

        await _context.SaveChangesAsync(ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return _mapper.Map<ColumnMetadata, ColumnMetadataDTO>(columnMetadata);
    }

    /// <summary>
    /// Creates a validation metadata for a column metadata. Replaces the existing.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="validationMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created validation metadata.</returns>
    public async Task<ColumnValidationMetadataDTO> SetValidationMetadata(int columnMetadataId, ColumnValidationMetadataDTO validationMetadata, CancellationToken ct = default)
    {
        var columnMetadata = await _context.Set<ColumnMetadata>()
            .Include(x => x.ValidationMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column with ID '{columnMetadataId}' doesn't exist.");

        await RemoveColumnValidationMetadataSet(columnMetadata, validationMetadata, ct);
        await _context.SaveChangesAsync(ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return _mapper.Map<ColumnValidationMetadata, ColumnValidationMetadataDTO>(columnMetadata.ValidationMetadata);
    }

    /// <summary>
    /// Creates a view metadata for a column metadata. Replaces the existing.
    /// </summary>
    /// <param name="columnMetadataId">The ID of a column metadata.</param>
    /// <param name="viewMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created view metadata.</returns>
    public async Task<ColumnViewMetadataDTO> SetViewMetadata(int columnMetadataId, ColumnViewMetadataDTO viewMetadata, CancellationToken ct = default)
    {
        var columnMetadata = await _context.Set<ColumnMetadata>()
            .Include(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct)
            ?? throw new ObjectNotExistsException($"The column with ID '{columnMetadataId}' doesn't exist.");

        await RemoveColumnViewMetadataSet(columnMetadata, viewMetadata, ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return _mapper.Map<ColumnViewMetadata, ColumnViewMetadataDTO>(columnMetadata.ViewMetadata);
    }

    /// <summary>
    /// Determines whether a table metadata exists.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<bool> TableMetadataExists(int id, CancellationToken ct = default) =>
        await _context.Set<TableMetadata>().AnyAsync(x => x.Id == id, ct);

    /// <summary>
    /// Updates a table metadata.
    /// </summary>
    /// <param name="id">The ID of a table metadata.</param>
    /// <param name="table">Data of a table metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated table metadata.</returns>
    public async Task<TableMetadataDTO> UpdateTableMetadata(int id, TableMetadataDTO table, CancellationToken ct = default)
    {
        var existingTableMetadata = await _context.Set<TableMetadata>().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ObjectNotExistsException($"The table with ID '{id}' doesn't exist.");

        if (table.TableId != existingTableMetadata.TableId)
            throw new BusinessException("Changing the unique table ID is not allowed.");

        if (table.FolderId != existingTableMetadata.FolderId)
            throw new BusinessException("Changing the parent folder is not allowed.");

        _mapper.Map(table, existingTableMetadata);
        await _context.SaveChangesAsync(ct);

        await SetCurrentChangedOnTime(existingTableMetadata.FolderId, ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return _mapper.Map<TableMetadata, TableMetadataDTO>(await TablesDeepQueryable().SingleAsync(x => x.Id == id, ct));
    }

    /// <summary>
    /// Updates a column metadata.
    /// </summary>
    /// <param name="id">The ID of a column metadata.</param>
    /// <param name="columnMetadataDto">Data of a column metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated column metadata.</returns>
    public async Task<ColumnMetadataDTO> UpdateColumnMetadata(int id, ColumnMetadataDTO columnMetadataDto, CancellationToken ct = default)
    {
        var existingColumnMetadata = await _context.Set<ColumnMetadata>().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new ObjectNotExistsException($"The column with ID '{id}' doesn't exist.");

        if (columnMetadataDto.ColumnId != existingColumnMetadata.ColumnId)
            throw new BusinessException("Changing the unique column ID is not allowed.");

        if (columnMetadataDto.TableId != existingColumnMetadata.TableId)
            throw new BusinessException("Changing the parent table is not allowed.");

        _mapper.Map(columnMetadataDto, existingColumnMetadata);
        await _context.SaveChangesAsync(ct);

        var parentTableMetadata = await _context.Set<TableMetadata>()
            .SingleOrDefaultAsync(x => x.Id == existingColumnMetadata.TableId, ct);
        await SetCurrentChangedOnTime(parentTableMetadata.FolderId, ct);

        await _dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return _mapper.Map<ColumnMetadata, ColumnMetadataDTO>(
            await ColumnsDeepQueryable().SingleAsync(x => x.Id == id, ct));
    }

    public async Task<IEnumerable<ColumnMetadataDTO>> GetTableColumns(string folderId, string tableId, CancellationToken ct)
    {
        var columnMetadata = await _context.Set<ColumnMetadata>()
            .Where(x => x.Table.TableId == tableId && x.Table.FolderId == Guid.Parse(folderId))
            .ToListAsync(ct);
        return _mapper.Map<List<ColumnMetadata>, List<ColumnMetadataDTO>>(columnMetadata);
    }

    private async Task SetCurrentChangedOnTime(Guid folderId, CancellationToken ct)
    {
        var folderInDb = await _context.Set<Folder>().SingleOrDefaultAsync(x => x.Id == folderId, ct)
            ?? throw new ConflictException($"The folder with ID {folderId} doesn't exist.");

        folderInDb.ChangedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    private IDictionary<string, ColumnMetadataResult> GetActualTableMetadata(TableMetadataDTO tableMetadataDto)
    {
        var result = new Dictionary<string, ColumnMetadataResult>();
        foreach (var columnMetadata in tableMetadataDto.Columns)
        {
            result.Add(columnMetadata.StaticData.PropertyName, new ColumnMetadataResult());
            if (columnMetadata.ColumnType is not null)
            {
                result[columnMetadata.StaticData.PropertyName].ValidationRules =
                    columnMetadata.ColumnType.ValidationMetadata?.Rules;
                result[columnMetadata.StaticData.PropertyName].GridColumnView =
                    columnMetadata.ColumnType.ViewMetadata?.GridColumnView;
            }
            else
            {
                result[columnMetadata.StaticData.PropertyName].ValidationRules =
                    columnMetadata.ValidationMetadata?.Rules;
                result[columnMetadata.StaticData.PropertyName].GridColumnView =
                    columnMetadata.ViewMetadata?.GridColumnView;
            }
        }

        return result;
    }

    private async Task RemoveTableMetadataEntity(TableMetadata table, CancellationToken ct)
    {
        var columnMetadataArray = table.Columns.ToArray();
        for (var index = columnMetadataArray.Length - 1; index >= 0; index--)
            await RemoveColumnMetadataEntity(columnMetadataArray[index], ct);

        _context.Set<TableMetadata>().Remove(table);
    }

    private async Task RemoveColumnMetadataEntity(ColumnMetadata column, CancellationToken ct)
    {
        if (column.ValidationMetadata is not null)
            _context.Set<ColumnValidationMetadata>().Remove(column.ValidationMetadata);

        if (column.ViewMetadata is not null)
            _context.Set<ColumnViewMetadata>().Remove(column.ViewMetadata);

        _context.Set<ColumnMetadata>().Remove(column);
    }

    private Task<Folder> GetDefaultFolderEntity(CancellationToken ct) =>
        _context.Set<Folder>().SingleOrDefaultAsync(x => x.Name == _dbDocFolderService.DefaultFolderName, ct);

    private async Task RemoveColumnValidationMetadataSet(ColumnMetadata columnMetadata, ColumnValidationMetadataDTO validationMetadataDto, CancellationToken ct)
    {
        if (columnMetadata.ValidationMetadataId is not null)
        {
            validationMetadataDto.Id = (int)columnMetadata.ValidationMetadataId;
            _mapper.Map(validationMetadataDto, columnMetadata.ValidationMetadata);
        }
        else
        {
            validationMetadataDto.Id = default;
            columnMetadata.ValidationMetadata = _mapper.Map<ColumnValidationMetadataDTO, ColumnValidationMetadata>(validationMetadataDto);
        }
    }

    private async Task RemoveColumnViewMetadataSet(ColumnMetadata columnMetadata, ColumnViewMetadataDTO viewMetadataDto, CancellationToken ct)
    {
        if (columnMetadata.ViewMetadata is not null)
        {
            viewMetadataDto.Id = columnMetadata.ViewMetadata.Id;

            if (viewMetadataDto.GridColumnView == null)
            {
                if (columnMetadata.ViewMetadata.GridColumnView != null)
                {
                    _context.Set<GridColumnView>().Remove(columnMetadata.ViewMetadata.GridColumnView);
                    await _context.SaveChangesAsync(ct);
                }
            }
            else
            {
                viewMetadataDto.GridColumnView.ColumnViewMetadataId = columnMetadata.ViewMetadata.Id;
                viewMetadataDto.GridColumnView.Id = columnMetadata.ViewMetadata.GridColumnView == null
                    ? default
                    : columnMetadata.ViewMetadata.GridColumnView.Id;
            }

            _mapper.Map(viewMetadataDto, columnMetadata.ViewMetadata);
        }
        else
        {
            viewMetadataDto.Id = default;
            columnMetadata.ViewMetadata = _mapper.Map<ColumnViewMetadataDTO, ColumnViewMetadata>(viewMetadataDto);

            if (columnMetadata.ViewMetadata.GridColumnView != null)
            {
                columnMetadata.ViewMetadata.GridColumnView.ColumnViewMetadata = null;
                columnMetadata.ViewMetadata.GridColumnView.ColumnViewMetadataId = viewMetadataDto.Id;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task RemoveColumnValidationMetadataEntity(ColumnMetadata columnMetadata, CancellationToken ct)
    {
        if (columnMetadata.ValidationMetadata is not null)
        {
            _context.Set<ColumnValidationMetadata>().Remove(columnMetadata.ValidationMetadata);
        }
    }

    private async Task RemoveColumnViewMetadataEntity(ColumnMetadata columnMetadata, CancellationToken ct)
    {
        if (columnMetadata.ViewMetadata is not null)
        {
            _context.Set<ColumnViewMetadata>().Remove(columnMetadata.ViewMetadata);
        }
    }

    private IQueryable<TableMetadata> TablesDeepQueryable() =>
        _context.Set<TableMetadata>()
            .Include(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .Include(x => x.Columns)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Columns)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView);

    private IQueryable<ColumnMetadata> ColumnsDeepQueryable() =>
        _context.Set<ColumnMetadata>()
            .Include(x => x.ColumnType)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.ColumnType)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .Include(x => x.ValidationMetadata)
            .Include(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .AsSplitQuery();
}