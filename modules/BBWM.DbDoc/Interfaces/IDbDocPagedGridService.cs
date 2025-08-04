using BBWM.Core.Filters;
using BBWM.DbDoc.Core.Classes;

namespace BBWM.DbDoc.Interfaces;

public interface IDbDocPagedGridService
{
    Task<TableDump> GetPage(string tableUid, Guid folderId, QueryCommand command, CancellationToken ct = default);

    Task DeleteRow(string uniqueTableId, object entityKey, CancellationToken ct = default);

    Task<dynamic> UpdateRow(dynamic entity, int tableMetadataId, CancellationToken ct = default);
}
