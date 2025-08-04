using BBWM.Core.Data;
using BBWM.Core.Filters;
using BBWM.DbDoc.Core.Classes;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.Interfaces;
using SqlKata;
using SqlKata.Execution;

namespace BBWM.DbDoc.Services;

/// <summary>
/// Queries DB table as paged grid using SQL. Currently only page getting method implemented.
/// Later can be extended with row CRUD methods.
/// </summary>
// TODO: we can move paged grid querying out of this service and make it core service, only based on
// connection, db type and table/columns names to query. This serivce will be only a wrapper, which fetches
// table's schema from db doc manager.
public class DbDocPagedGridService : IDbDocPagedGridService
{
    private readonly IDbSchemaManager _dbSchemaManager;

    public DbDocPagedGridService(IDbSchemaManager dbSchemaManager) => _dbSchemaManager = dbSchemaManager;

    public async Task<TableDump> GetPage(string tableUid, Guid folderId, QueryCommand command,
        CancellationToken ct = default)
    {
        var dbSource = _dbSchemaManager.GetTableDbSchema(tableUid).DatabaseSource;
        var columns = _dbSchemaManager.GetTableColumns(tableUid);
        var tableName = columns.First().ParentTableName;

        var result = new TableDump
        {
            Columns = columns.Select(x => new Tuple<string, string>(x.ColumnName, x.ColumnName)).ToList()
        };

        var queryFactory = SqlKataHelper.GetQueryFactory(dbSource.ConnectionString, dbSource.DatabaseType);

        #region Get paged rows
        var queryRows = new Query(tableName)
            .Select(columns.Select(x => x.ColumnName));

        if (command.Take > 0)
            queryRows.Take(command.Take.Value);

        if (command.Skip > 0)
            queryRows.Skip(command.Skip.Value);

        if (!string.IsNullOrEmpty(command.SortingField))
        {
            if (command.SortingDirection == OrderDirection.Asc)
                queryRows.OrderBy(command.SortingField);
            else
                queryRows.OrderByDesc(command.SortingField);
        }

        var rows = await queryFactory.GetAsync(queryRows, cancellationToken: ct);        
        #endregion

        #region Get total
        var queryTotal = new Query(tableName);
        var total = queryFactory.ExecuteScalar<int>(queryTotal.AsCount());
        #endregion

        result.Data = new PageResult<dynamic>
        {
            Items = rows,
            Total = total
        };

        return result;
    }

    public async Task DeleteRow(string tableUid, object entityKey, CancellationToken ct = default)
        => throw new NotImplementedException();

    public async Task<dynamic> UpdateRow(dynamic entity, int tableMetadataId, CancellationToken ct = default)
        => throw new NotImplementedException();
}