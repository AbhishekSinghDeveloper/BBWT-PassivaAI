using BBF.Reporting.TableSet.Connectors.DbDoc;
using BBF.Reporting.TableSet.Connectors.Forms;
using BBF.Reporting.TableSet.DTO;
using BBF.Reporting.TableSet.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.FormIO.Connectors.ReportingV3;

namespace BBF.Reporting.TableSet.Services;

public class TableSetService : ITableSetService
{
    private readonly IDbDocService _dbDocService;
    private readonly IDbSchemaManager _dbSchemaManager;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IQueryableFormsService _queryableFormsService;

    public TableSetService(
        IDbDocService dbDocService,
        IDbSchemaManager dbSchemaManager,
        IDbDocFolderService dbDocFolderService,
        IQueryableFormsService queryableFormsService)
    {
        _dbDocService = dbDocService;
        _dbSchemaManager = dbSchemaManager;
        _dbDocFolderService = dbDocFolderService;
        _queryableFormsService = queryableFormsService;
    }

    // Returns database source corresponding to the given source code.
    public async Task<DatabaseSource> GetQueryDbSource(string sourceCode, string folderId, CancellationToken ct = default)
    {
        switch (sourceCode)
        {
            case DbDocConnector.SourceCode:
            {
                if (folderId is null)
                    throw new ObjectNotExistsException("Cannot find folder with given folder id.");

                var dbSourceId = await _dbDocFolderService.GetFolderDatabaseSourceId(Guid.Parse(folderId), ct);
                if (dbSourceId == null || dbSourceId == Guid.Empty)
                    throw new ObjectNotExistsException($"Database for this folder is not found. " +
                                                       $"Note: current reporting version supports only DB " +
                                                       $"Documenting folders created by connected " +
                                                       $"database. Custom folders are not supported yet.");

                return _dbSchemaManager.GetDbSchema(dbSourceId.Value).DatabaseSource;
            }

            // In this version, the Forms module only uses the main DB.
            default:
                return _dbSchemaManager.GetMainDbSchema().DatabaseSource;
        }
    }

    // For now, we directly select from DbDoc folders.
    // It can be improved to get folders from Connectors (DbDoc / Forms).
    public async Task<IEnumerable<TableSetFolderDTO>> GetFolders(CancellationToken ct = default)
    {
        // Collecting folders from DbDoc.
        var dbDocFolders = await _dbDocFolderService.GetOwnerFolders(DbDocConnector.DbDocFolderOwnerName, ct);

        var tableSetFolders = dbDocFolders.Select(folder => new TableSetFolderDTO
        {
            Id = folder.Id.ToString(),
            Name = folder.Name,
            SourceCode = DbDocConnector.SourceCode
        }).ToList();

        // Collecting folders from Forms.
        tableSetFolders.Add(new TableSetFolderDTO
        {
            Id = new Guid().ToString(),
            Name = "Forms",
            SourceCode = FormsConnector.SourceCode
        });

        return tableSetFolders;
    }

    // For now, we directly select from DB DOC folders.
    // It can be improved to get folders from Connectors (DbDoc / Forms).
    public async Task<IEnumerable<TableSetTableDTO>> GetFolderTables(string sourceCode, string folderId, CancellationToken ct = default)
    {
        switch (sourceCode)
        {
            case DbDocConnector.SourceCode:
                var dbDocTables = await _dbDocFolderService.GetFolderTableMatadata(Guid.Parse(folderId), ct);
                var sourcesSpecificDbEntities = await GetAllSourcesSpecificDbEntities(ct);

                // To ensure, we remain only table metadata which have DB schema.
                // But good solution for future is - avoid table metadata records not be out of sync with DB schema
                return dbDocTables
                    .Where(table => table.StaticData != null && !sourcesSpecificDbEntities.Contains(table.StaticData.TableName))
                    .Select(table => GetTableSetTable(folderId, table)).ToList();

            case FormsConnector.SourceCode:
                var queryableForms = await _queryableFormsService.GetQueryableForms(false, false, ct);
                return queryableForms.Select(form => GetTableSetTable(folderId, form)).ToList();

            default: throw new NotSupportedException();
        }
    }

    public async Task<TableSetTableDTO?> GetTable(string sourceCode, string folderId, string tableId,
        string? parentTableId, CancellationToken ct = default)
    {
        switch (sourceCode)
        {
            case DbDocConnector.SourceCode:
                var dbDocTable = await GetDbDocTable(folderId, tableId, ct);
                return dbDocTable == null ? null : GetTableSetTable(folderId, dbDocTable);

            case FormsConnector.SourceCode:
                var queryableForm = await _queryableFormsService.GetQueryableForm(tableId, parentTableId, ct);
                return queryableForm == null ? null : GetTableSetTable(folderId, queryableForm);

            default: throw new NotSupportedException();
        }
    }

    public async Task<IEnumerable<TableSetColumnDTO>> GetTableColumns(string sourceCode, string folderId,
        string tableId, string? parentTableId, CancellationToken ct = default)
    {
        switch (sourceCode)
        {
            case DbDocConnector.SourceCode:
                var dbDocColumns = await _dbDocService.GetTableColumns(folderId, tableId, ct);
                return dbDocColumns
                    .Where(column => !column.Hidden)
                    .Select(column => GetTableSetColumn(tableId, column));

            case FormsConnector.SourceCode:
                var queryableForm = await _queryableFormsService.GetQueryableForm(tableId, parentTableId, ct);
                return queryableForm?.Columns.Select(column => GetTableSetColumn(tableId, column)) ?? Enumerable.Empty<TableSetColumnDTO>();

            default: throw new NotSupportedException();
        }
    }

    private async Task<TableMetadataDTO?> GetDbDocTable(string folderId, string tableId, CancellationToken ct)
    {
        var tableIds = new[] { tableId };
        var tables = await _dbDocFolderService.GetFullTablesMatadata(Guid.Parse(folderId), tableIds, ct);
        var table = tables.FirstOrDefault();

        // Load table columns only if it is necessary.
        if (table is { Columns: not { Count: > 0 } })
            table.Columns = (await _dbDocService.GetTableColumns(folderId, tableId, ct)).ToList();

        return table;
    }

    private static TableSetTableDTO GetTableSetTable(string folderId, TableMetadataDTO dbDocTable)
    {
        var columns = dbDocTable.Columns
            .Where(column => !column.Hidden)
            .Select(column => GetTableSetColumn(dbDocTable.TableId, column));

        return new TableSetTableDTO
        {
            Id = dbDocTable.TableId,
            Name = dbDocTable.StaticData.TableName,
            TableAlias = dbDocTable.StaticData.TableName,
            SourceCode = DbDocConnector.SourceCode,
            FolderId = folderId,
            Columns = columns
        };
    }

    private static TableSetColumnDTO GetTableSetColumn(string tableId, ColumnMetadataDTO dbDocColumn)
    {
        return new TableSetColumnDTO
        {
            Id = dbDocColumn.ColumnId,
            Name = dbDocColumn.StaticData.ColumnName,
            ColumnAlias = dbDocColumn.StaticData.ColumnName,
            IsPrimaryKey = dbDocColumn.StaticData.IsPrimaryKey ?? false,
            IsForeignKey = dbDocColumn.StaticData.IsForeignKey ?? false,
            TableId = tableId
        };
    }

    private static TableSetTableDTO GetTableSetTable(string folderId, QueryableForm queryableForm)
    {
        var columns = queryableForm.Columns
            .Select(column => GetTableSetColumn(queryableForm.Id, column));
        var children = queryableForm.Children
            .Select(column => GetTableSetTable(folderId, column));

        return new TableSetTableDTO
        {
            Id = queryableForm.Id,
            Name = queryableForm.FormName,
            TableAlias = queryableForm.TableAlias,
            SourceCode = FormsConnector.SourceCode,
            ParentTableId = queryableForm.ParentFormId,
            Children = children,
            FolderId = folderId,
            Columns = columns
        };
    }

    private static TableSetColumnDTO GetTableSetColumn(string tableId, QueryableFormColumn queryableFormColumn)
    {
        return new TableSetColumnDTO
        {
            Id = queryableFormColumn.Id,
            Name = queryableFormColumn.FormName,
            ColumnAlias = queryableFormColumn.ColumnAlias,
            IsPrimaryKey = queryableFormColumn.IsPrimaryKey,
            IsForeignKey = queryableFormColumn.IsForeignKey,
            TableId = tableId
        };
    }

    private async Task<IEnumerable<string>> GetAllSourcesSpecificDbEntities(CancellationToken ct)
    {
        var result = new List<string>();
        result.AddRange(await GetSourceOnlySpecificDbEntities(DbDocConnector.SourceCode, ct));
        result.AddRange(await GetSourceOnlySpecificDbEntities(FormsConnector.SourceCode, ct));
        return result;
    }

    private async Task<IEnumerable<string>> GetSourceOnlySpecificDbEntities(string sourceCode, CancellationToken ct)
    {
        switch (sourceCode)
        {
            // DB DOC itself contains DB entities for general purposes, therefore it has no DB DOC specific entities.
            case DbDocConnector.SourceCode: return new List<string>();

            case FormsConnector.SourceCode:
                var queryableForms = await _queryableFormsService.GetQueryableForms(false, true, ct);
                return queryableForms.Select(form => form.TableAlias);

            default: throw new NotSupportedException();
        }
    }
}