using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;

namespace BBWM.DbDoc.Services;

public class ColumnTypeService : IColumnTypeService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDataService _dataService;
    private readonly IDbDocGitLabService _gitLabService;


    public ColumnTypeService(
        IDbContext context,
        IMapper mapper,
        IDataService dataService,
        IDbDocGitLabService gitLabService)
    {
        _context = context;
        _mapper = mapper;
        _dataService = dataService;
        _gitLabService = gitLabService;
    }


    public IQueryable<ColumnType> GetEntityQuery(IQueryable<ColumnType> baseQuery) => baseQuery
            .Include(x => x.ValidationMetadata)
            .Include(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView);

    public async Task<IEnumerable<ColumnTypeDTO>> GetAll(CancellationToken ct) =>
        _mapper.Map<IEnumerable<ColumnType>, IEnumerable<ColumnTypeDTO>>(
            await GetEntityQuery(_context.Set<ColumnType>()).ToListAsync(ct));

    public async Task<ColumnTypeDTO> Create(ColumnTypeDTO dto, CancellationToken ct = default)
    {
        // DTO input cleaup
        dto.Name = dto.Name.Trim();

        if (await _dataService.Any<ColumnType>(query => query.Where(x => x.Name == dto.Name), ct))
            throw new BusinessException($"The column type with name '{dto.Name}' already exists.");

        var result = await _dataService.Create<ColumnType, ColumnTypeDTO>(dto, ct);

        await _gitLabService.SendCurrentDbDocStateToGit(false, ct);

        return result;
    }

    public async Task<ColumnTypeDTO> Update(ColumnTypeDTO dto, CancellationToken ct = default)
    {
        // DTO input cleaup
        dto.Name = dto.Name.Trim();

        if (await _dataService.Any<ColumnType>(query => query.Where(x => x.Name == dto.Name && x.Id != dto.Id), ct))
            throw new BusinessException($"The column type with name '{dto.Name}' already exists.");

        var result = await _dataService.Update<ColumnType, ColumnTypeDTO, Guid>(dto, ct);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);

        return result;
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        var columnType = await GetEntityQuery(_context.Set<ColumnType>()).SingleOrDefaultAsync(x => x.Id == id, ct);

        if (columnType.ValidationMetadata != null)
            _context.Set<ColumnValidationMetadata>().Remove(columnType.ValidationMetadata);

        if (columnType.ViewMetadata != null)
            _context.Set<ColumnViewMetadata>().Remove(columnType.ViewMetadata);

        await _dataService.Delete<ColumnType, Guid>(id, ct);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);
    }

    /// <summary>
    /// Deletes a validation metadata from a column type.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteValidationMetadata(Guid columnTypeId, CancellationToken ct)
    {
        var columnType = await _context.Set<ColumnType>()
            .Include(x => x.ValidationMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnTypeId, ct);
        if (columnType == null)
            throw new ObjectNotExistsException($"The column type with ID '{columnTypeId}' doesn't exist.");

        if (columnType.ValidationMetadata == null) return;

        _context.Set<ColumnValidationMetadata>().Remove(columnType.ValidationMetadata);
        await _context.SaveChangesAsync(ct);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);
    }

    /// <summary>
    /// Deletes a view metadata from a column type.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task DeleteViewMetadata(Guid columnTypeId, CancellationToken ct)
    {
        var columnType = await _context.Set<ColumnType>()
            .Include(x => x.ViewMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnTypeId, ct);
        if (columnType == null)
            throw new ObjectNotExistsException($"The column type with ID '{columnTypeId}' doesn't exist.");

        if (columnType.ViewMetadata == null) return;

        _context.Set<ColumnViewMetadata>().Remove(columnType.ViewMetadata);
        await _context.SaveChangesAsync(ct);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);
    }

    /// <summary>
    /// Creates validation metadata for a column type. Replaces the existing.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="validationMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created validation metadata.</returns>
    public async Task<ColumnValidationMetadataDTO> SetValidationMetadata(Guid columnTypeId, ColumnValidationMetadataDTO validationMetadata, CancellationToken ct)
    {
        var columnType = await _context.Set<ColumnType>()
            .Include(x => x.ValidationMetadata)
            .SingleOrDefaultAsync(x => x.Id == columnTypeId);

        if (columnType == null)
            throw new ObjectNotExistsException($"The column type with ID '{columnTypeId}' doesn't exist.");

        if (columnType.ValidationMetadata != null)
        {
            validationMetadata.Id = columnType.ValidationMetadata.Id;
            _mapper.Map(validationMetadata, columnType.ValidationMetadata);
        }
        else
        {
            validationMetadata.Id = default;
            columnType.ValidationMetadata = _mapper.Map<ColumnValidationMetadataDTO, ColumnValidationMetadata>(validationMetadata);
        }

        await _context.SaveChangesAsync(ct);

        var result = _mapper.Map<ColumnValidationMetadata, ColumnValidationMetadataDTO>(columnType.ValidationMetadata);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);

        return result;
    }

    /// <summary>
    /// Creates view metadata for a column type. Replaces the existing.
    /// </summary>
    /// <param name="columnTypeId">The ID of a column type.</param>
    /// <param name="viewMetadata">Saving data object.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Created view metadata.</returns>
    public async Task<ColumnViewMetadataDTO> SetViewMetadata(Guid columnTypeId, ColumnViewMetadataDTO viewMetadata, CancellationToken ct)
    {
        var columnType = await _context.Set<ColumnType>()
            .Include(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .SingleOrDefaultAsync(x => x.Id == columnTypeId);

        if (columnType == null)
            throw new ObjectNotExistsException($"The column type with ID '{columnTypeId}' doesn't exist.");

        if (columnType.ViewMetadata != null)
        {
            viewMetadata.Id = columnType.ViewMetadata.Id;

            if (viewMetadata.GridColumnView == null)
            {
                if (columnType.ViewMetadata.GridColumnView != null)
                {
                    _context.Set<GridColumnView>().Remove(columnType.ViewMetadata.GridColumnView);
                    await _context.SaveChangesAsync(ct);
                }
            }
            else
            {
                viewMetadata.GridColumnView.ColumnViewMetadataId = columnType.ViewMetadata.Id;
                viewMetadata.GridColumnView.Id = columnType.ViewMetadata.GridColumnView == null
                    ? default
                    : columnType.ViewMetadata.GridColumnView.Id;
            }

            _mapper.Map(viewMetadata, columnType.ViewMetadata);
        }
        else
        {
            viewMetadata.Id = default;
            columnType.ViewMetadata = _mapper.Map<ColumnViewMetadataDTO, ColumnViewMetadata>(viewMetadata);

            if (columnType.ViewMetadata.GridColumnView != null)
            {
                columnType.ViewMetadata.GridColumnView.ColumnViewMetadata = null;
                columnType.ViewMetadata.GridColumnView.ColumnViewMetadataId = viewMetadata.Id;
            }
        }

        await _context.SaveChangesAsync(ct);

        var result = _mapper.Map<ColumnViewMetadata, ColumnViewMetadataDTO>(columnType.ViewMetadata);

        await _gitLabService.SendCurrentDbDocStateToGit(ct);

        return result;
    }
}
