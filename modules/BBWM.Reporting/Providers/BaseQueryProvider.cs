using BBWM.Core.Exceptions;
using BBWM.Reporting.Extensions;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;

using System.Text;

namespace BBWM.Reporting.Providers;

public class BaseQueryProvider
{
    public virtual SqlKata.Query BuildBaseQuery(Query query, QueryTablesSchema tablesSchema)
    {
        if (!query.QueryTables.Any())
        {
            throw new BusinessException("The query doesn't contain query tables.");
        }

        var sqlKataQuery = new SqlKata.Query(GetQueryTableQueryName(query.QueryTables.First(), tablesSchema));

        AddQueryTableJoins(sqlKataQuery, query, tablesSchema, query.QueryTables.First().Id, new List<int>());

        return sqlKataQuery;
    }


    private void AddQueryTableJoins(SqlKata.Query finalQuery, Query query, QueryTablesSchema tablesSchema, int queryTableId, IList<int> visitedJoins)
    {
        var notVisitedJoins = GetJoinsOfQueryTable(query, queryTableId).Where(x => !visitedJoins.Contains(x.Id)).ToList();

        foreach (var queryTableJoin in notVisitedJoins)
        {
            var fromQueryTable = queryTableJoin.FromQueryTableId == queryTableId ? queryTableJoin.FromQueryTable : queryTableJoin.ToQueryTable;
            var toQueryTable = queryTableJoin.FromQueryTableId == queryTableId ? queryTableJoin.ToQueryTable : queryTableJoin.FromQueryTable;

            var toTableName = GetQueryTableQueryName(queryTableJoin.FromQueryTableId == queryTableId ? queryTableJoin.ToQueryTable : queryTableJoin.FromQueryTable, tablesSchema);

            var fromColumnName = GetQueryTableColumnQueryName(
                queryTableJoin.FromQueryTableId == queryTableId ? queryTableJoin.FromQueryTableColumn : queryTableJoin.ToQueryTableColumn,
                tablesSchema,
                fromQueryTable.Alias);

            var toColumnName = GetQueryTableColumnQueryName(
                queryTableJoin.FromQueryTableId == queryTableId ? queryTableJoin.ToQueryTableColumn : queryTableJoin.FromQueryTableColumn,
                tablesSchema,
                toQueryTable.Alias);

            switch (queryTableJoin.JoinType)
            {
                case Enums.QueryJoinTypeEnum.Inner:
                    finalQuery.Join(toTableName, toColumnName, fromColumnName);
                    break;
                case Enums.QueryJoinTypeEnum.Left:
                    finalQuery.LeftJoin(toTableName, toColumnName, fromColumnName);
                    break;
            }

            visitedJoins.Add(queryTableJoin.Id);
        }

        foreach (var queryTableJoin in notVisitedJoins)
        {
            AddQueryTableJoins(
                finalQuery,
                query,
                tablesSchema,
                queryTableJoin.FromQueryTableId == queryTableId ? (int)queryTableJoin.ToQueryTableId : (int)queryTableJoin.FromQueryTableId,
                visitedJoins);
        }
    }

    private IEnumerable<QueryTableJoin> GetJoinsOfQueryTable(Query query, int queryTableId) =>
        query.QueryTableJoins.Where(x => x.FromQueryTableId == queryTableId || x.ToQueryTableId == queryTableId).ToList();

    private string GetQueryTableColumnQueryName(QueryTableColumn queryTableColumn, QueryTablesSchema tablesSchema, string queryTableAlias = "")
    {
        var sourceColumn = tablesSchema.GetColumn(queryTableColumn);
        return string.IsNullOrEmpty(queryTableAlias) ? sourceColumn.QueryName : $"{queryTableAlias}.{sourceColumn.ColumnName}";
    }

    private string GetQueryTableQueryName(QueryTable queryTable, QueryTablesSchema tablesSchema)
    {
        var result = new StringBuilder(tablesSchema.GetTable(queryTable).QueryName);

        if (!string.IsNullOrEmpty(queryTable.Alias))
        {
            result.Append($" AS {queryTable.Alias}");
        }

        return result.ToString();
    }
}
