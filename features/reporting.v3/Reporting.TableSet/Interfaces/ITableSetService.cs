using BBF.Reporting.TableSet.DTO;
using BBWM.DbDoc.Model;

namespace BBF.Reporting.TableSet.Interfaces;

public interface ITableSetService
{
    Task<DatabaseSource> GetQueryDbSource(string sourceCode, string folderId, CancellationToken ct = default);

    Task<IEnumerable<TableSetFolderDTO>> GetFolders(CancellationToken ct = default);

    Task<IEnumerable<TableSetTableDTO>> GetFolderTables(string sourceCode, string folderId, CancellationToken ct = default);

    Task<TableSetTableDTO?> GetTable(string sourceCode, string folderId, string tableId,
        string? parentTableId, CancellationToken ct = default);

    Task<IEnumerable<TableSetColumnDTO>> GetTableColumns(string sourceCode, string folderId, string tableId,
        string? parentTableId, CancellationToken ct = default);
}