using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbMacros;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;

namespace BBWM.DbDoc.Services;

public class DbDocFolderService : IDbDocFolderService
{
    private readonly IDbContext context;
    private readonly IDbSchemaManager _dbSchemaManager;
    private readonly IDbDocGitLabService dbDocGitLabService;
    private readonly IDbPathMacroService dbPathMacroService;
    private readonly IMapper mapper;

    public DbDocFolderService(
        IDbContext _context,
        IDbSchemaManager _dbSchemaManager,
        IDbDocGitLabService _dbDocGitLabService,
        IDbPathMacroService _dbPathMacroService,
        IMapper _mapper)
    {
        context = _context;
        this._dbSchemaManager = _dbSchemaManager;
        dbDocGitLabService = _dbDocGitLabService;
        dbPathMacroService = _dbPathMacroService;
        mapper = _mapper;
    }

    // TODO: remove it. Use only inline on main folder seeded
    public string DefaultFolderName => "All Tables";

    public async Task<FolderDTO> CreateFolder(FolderDTO folderDto, CancellationToken ct = default)
    {
        if (await context.Set<Folder>().AnyAsync(x => x.Name == folderDto.Name, ct))
            throw new BusinessException($"The folder with name '{folderDto.Name}' already exists.");

        if (string.IsNullOrWhiteSpace(folderDto.Name))
            throw new BusinessException("Folder name required.");

        var folder = mapper.Map<FolderDTO, Folder>(folderDto);
        folder.Id = default;
        folder.ChangedOn = DateTime.UtcNow;
        await context.Set<Folder>().AddAsync(folder, ct);
        await context.SaveChangesAsync(ct);

        await dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return mapper.Map<Folder, FolderDTO>(folder);
    }

    public async Task<IEnumerable<FolderDTO>> DeleteFolder(Guid folderId, CancellationToken ct = default)
    {
        var folder = await FoldersDeepQueryable().SingleOrDefaultAsync(x => x.Id == folderId, ct)
            ?? throw new ObjectNotExistsException($"The folder with ID '{folderId}' doesn't exist.");

        if (folder.Name == DefaultFolderName)
            throw new BusinessException("The default folder can not be deleted.");

        var removedTables = await RemoveReferencedTableEntities(folder, ct);
        var affectedFolderIds = removedTables.Select(x => x.FolderId).Distinct().ToList();

        RemoveFolderTableEntities(folder);
        context.Set<Folder>().Remove(folder);

        await context.SaveChangesAsync(ct);

        if (folder.DatabaseSourceId is not null
            && !await context.Set<Folder>().AnyAsync(x => x.DatabaseSourceId == folder.DatabaseSourceId))
            await _dbSchemaManager.UnregisterDatabaseSource(folder.DatabaseSourceId.Value, ct);

        await dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        var affectedFolders = await FoldersDeepQueryable()
            .Where(x => affectedFolderIds.Contains(x.Id))
            .ToListAsync(ct);
        return mapper.Map<List<Folder>, List<FolderDTO>>(affectedFolders);
    }

    public async Task<bool> FolderExists(Guid folderId, CancellationToken ct = default) =>
        await context.Set<Folder>().AnyAsync(x => x.Id == folderId, ct);

    public async Task AddMainDbFolderOwner(string owner, CancellationToken ct = default)
    {
        var folderId = await context.Set<Folder>()
            .Where(x => x.DatabaseSource.ContextId == _dbSchemaManager.MainDbContextId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync(ct);

        await AddFolderOwner(folderId, owner, ct);
    }

    public async Task AddFolderOwner(Guid folderId, string owner, CancellationToken ct = default)
    {
        var dto = mapper.Map<Folder, FolderDTO>(await context.Set<Folder>()
            .SingleOrDefaultAsync(x => x.Id == folderId, ct));
        if (dto.Owners.All(x => x != owner))
        {
            dto.Owners.Add(owner);
        }
        await UpdateFolder(folderId, dto, ct);
    }

    public async Task<IEnumerable<FolderDTO>> GetOwnerFolders(string owner, CancellationToken ct = default) =>
        mapper.Map<IEnumerable<Folder>, IEnumerable<FolderDTO>>(
            await context.Set<Folder>().Where(x => x.Owners.Contains(owner)).ToListAsync(ct));

    public async Task<IEnumerable<TableMetadataDTO>> GetFolderTableMatadata(Guid folderId, CancellationToken ct = default) =>
        mapper.Map<IEnumerable<TableMetadata>, IEnumerable<TableMetadataDTO>>(
            await context.Set<TableMetadata>().Where(x => x.FolderId == folderId).ToListAsync(ct));

    public async Task<IEnumerable<TableMetadataDTO>> GetFullTablesMatadata(
        Guid folderId,
        IEnumerable<string> tableMetadataIds,
        CancellationToken ct = default) =>
        mapper.Map<IEnumerable<TableMetadata>, IEnumerable<TableMetadataDTO>>(
            await TablesDeepQueryable().Where(x => x.FolderId == folderId && tableMetadataIds.Contains(x.TableId))
            .AsNoTracking()
            .ToListAsync(ct));

    public async Task<IEnumerable<FolderDTO>> GetDbExplorerFolders(CancellationToken ct = default) =>
        mapper.Map<IEnumerable<Folder>, IEnumerable<FolderDTO>>(
            await (context.Set<Folder>().Include(x => x.DatabaseSource)).ToListAsync(ct));

    public async Task<FolderDTO> GetFolder(Guid folderId, CancellationToken ct = default) =>
        mapper.Map<Folder, FolderDTO>(
            await FoldersDeepQueryable().AsNoTracking().SingleOrDefaultAsync(x => x.Id == folderId, ct));

    public async Task<Guid?> GetFolderDatabaseSourceId(Guid folderId, CancellationToken ct = default) =>
        await context.Set<Folder>()
                .Where(x => x.Id == folderId)
                .Select(x => x.DatabaseSourceId)
                .SingleOrDefaultAsync(ct);

    // TODO: remove it when Reporting v2 removed
    public async Task<FolderDTO> GetDefaultFolder(CancellationToken ct = default) =>
        mapper.Map<Folder, FolderDTO>(
            await FoldersDeepQueryable().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Name == DefaultFolderName, ct));

    public async Task<FolderDTO> UpdateFolder(Guid folderId, FolderDTO folder, CancellationToken ct = default)
    {
        var existingDbDocFolder = await context.Set<Folder>().SingleOrDefaultAsync(x => x.Id == folderId, ct)
            ?? throw new ObjectNotExistsException($"The folder with ID '{folderId}' doesn't exist.");

        var defaultFolder = await context.Set<Folder>().SingleOrDefaultAsync(x => x.Name == DefaultFolderName, ct);
        if (defaultFolder.Id == folder.Id && defaultFolder.Name != folder.Name)
            throw new BusinessException("Changing the default folder name is not allowed.");

        if (string.IsNullOrWhiteSpace(folder.Name))
            throw new BusinessException("Folder name can not be empty.");

        mapper.Map(folder, existingDbDocFolder);
        existingDbDocFolder.ChangedOn = DateTime.UtcNow;

        if (await context.Set<Folder>().AnyAsync(x => x.Name == folder.Name && x.Id != existingDbDocFolder.Id, ct))
            throw new BusinessException($"The folder with name '{folder.Name}' already exists.");

        await context.SaveChangesAsync(ct);

        await dbDocGitLabService.SendCurrentDbDocStateToGit(ct);

        return await GetFolder(existingDbDocFolder.Id, ct);
    }

    public async Task<IEnumerable<DbPathMacroDTO>> GetDbPathMacrosAllAliases(Guid folderId, CancellationToken ct = default)
    {
        var folderDbSourceId = await context.Set<Folder>()
            .Where(x => x.Id == folderId)
            .Select(x => x.DatabaseSourceId)
            .FirstOrDefaultAsync(ct);

        var macros = dbPathMacroService.GetPathMacrosAllAliases(folderDbSourceId.Value).ToList();

        return macros.Select(x => new DbPathMacroDTO
        {
            Definition = x.Definition,
            Path = x.Path.Any() ?
                x.Path
                    .Select(y => y.Data.StartTableColumn)
                    .Append(x.Path.Last().Data.EndTableColumn)
                    .Select(y => new DbPathNodeDTO
                    {
                        TableName = y.ParentTableName,
                        ColumnName = y.ColumnName,
                    })
                    .ToList()
                : new List<DbPathNodeDTO>()
        });
    }

    public async Task RemoveFolderTables(Guid folderId, CancellationToken ct)
    {
        var folder = await FoldersDeepQueryable().SingleOrDefaultAsync(x => x.Id == folderId, ct);
        RemoveFolderTableEntities(folder);
        await context.SaveChangesAsync(ct);
    }

    private async Task<List<TableMetadata>> RemoveReferencedTableEntities(Folder targetFolder, CancellationToken ct)
    {
        // If it's not a database source based folder then no other folder's table can reference it
        if (!targetFolder.IsSourceFolder)
            return new List<TableMetadata>();

        var targetTables = targetFolder.Tables.ToArray();
        var removedTables = new List<TableMetadata>();

        for (var it = targetTables.Length - 1; it >= 0; it--)
        {
            var referenceTables = await TablesDeepQueryable()
                .Where(x => x.TableId == targetTables[it].TableId && x.FolderId != targetFolder.Id)
                .ToArrayAsync(ct);

            for (var ir = referenceTables.Length - 1; ir >= 0; ir--)
            {
                RemoveTableMetadataEntity(referenceTables[ir]);
                removedTables.Add(referenceTables[ir]);
            }
        }

        return removedTables;
    }

    private void RemoveFolderTableEntities(Folder folder)
    {
        var tables = folder.Tables.ToArray();
        for (var i = tables.Length - 1; i >= 0; i--)
            RemoveTableMetadataEntity(tables[i]);
    }

    private void RemoveTableMetadataEntity(TableMetadata table)
    {
        var columnMetadataArray = table.Columns.ToArray();
        for (var index = columnMetadataArray.Length - 1; index >= 0; index--)
            RemoveColumnMetadataEntity(columnMetadataArray[index]);

        context.Set<TableMetadata>().Remove(table);
    }

    private void RemoveColumnMetadataEntity(ColumnMetadata column)
    {
        if (column.ValidationMetadata is not null)
            context.Set<ColumnValidationMetadata>().Remove(column.ValidationMetadata);

        if (column.ViewMetadata is not null)
            context.Set<ColumnViewMetadata>().Remove(column.ViewMetadata);

        context.Set<ColumnMetadata>().Remove(column);
    }

    private IQueryable<Folder> FoldersDeepQueryable() =>
        context.Set<Folder>()
            .Include(x => x.DatabaseSource)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ColumnType)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ValidationMetadata)
            .Include(x => x.Tables)
            .ThenInclude(x => x.Columns)
            .ThenInclude(x => x.ViewMetadata)
            .ThenInclude(x => x.GridColumnView)
            .AsSplitQuery();

    private IQueryable<TableMetadata> TablesDeepQueryable() =>
        context.Set<TableMetadata>()
            .Include(x => x.Folder)
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
}