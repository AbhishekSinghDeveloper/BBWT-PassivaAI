using BBWM.Core.Filters;
using BBWM.Reporting.Interfaces;
using System.Net;

namespace BBWM.Reporting.Providers;

public class OrderActionProvider
{
    private readonly QueryTablesSchema tablesSchema;

    public OrderActionProvider(QueryTablesSchema tablesSchema)
    {
        this.tablesSchema = tablesSchema;
    }

    public void Apply(SqlKata.Query sqlKataQuery, QueryCommand queryCommand = null)
    {
        if (string.IsNullOrWhiteSpace(queryCommand?.SortingField)) return;

        // TODO: it can be a better place to decode it - tthe same as for FilterInfoModelBinder - in model filters
        var decodedQueryCommandField = WebUtility.UrlDecode(queryCommand.SortingField);

        var sortingField = tablesSchema.Columns
            .SingleOrDefault(x => x.GetQueryAlias() == decodedQueryCommandField)
            .QueryName;

        if (string.IsNullOrEmpty(sortingField)) return;

        var sortingOrder = queryCommand.SortingDirection ?? OrderDirection.Asc;

        if (sortingOrder == OrderDirection.Asc)
            sqlKataQuery.OrderBy(sortingField);
        else
            sqlKataQuery.OrderByDesc(sortingField);
    }
}
