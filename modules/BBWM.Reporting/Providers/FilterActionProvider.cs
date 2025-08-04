using BBWM.Core.Extensions;
using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.DbDoc.Enums;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Extensions;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;

using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BBWM.Reporting.Providers;

public class FilterActionProvider
{
    private readonly Query _query;
    private readonly QueryTablesSchema _tablesSchema;
    private readonly IEnumerable<QueryTableColumn> _queryTableColumns;


    public FilterActionProvider(Query query, QueryTablesSchema tablesSchema)
    {
        _query = query;
        _tablesSchema = tablesSchema;
        _queryTableColumns = query.QueryTables.SelectMany(qt => qt.Columns);
    }


    public void Apply(SqlKata.Query sqlKataQuery, QueryCommand queryCommand = null)
    {
        if (_query.RootFilterSet.ConditionalOperator == QueryConditionalOperator.And)
        {
            sqlKataQuery.Where(x => ApplyFilterSet(_query.RootFilterSet, queryCommand));
        }
        else
        {
            sqlKataQuery.OrWhere(x => ApplyFilterSet(_query.RootFilterSet, queryCommand));
        }
    }

    /// <summary>
    /// When query filter binding is <see cref="QueryFilterBindingType.MasterDetailGrid"/> then for grid paging
    /// we need to pass the filter's identifier from client-side. We use ReportingQueryFilterBindings record's ID for that.
    /// Here we generate unique ID avoiding mixing them with filters linked to UI filter controls
    /// </summary>
    private const string QueryFilterBindingNamePrefix = "query-filter-binding-";
    public static string QueryFilterBindingAsFilterId(int bindingId)
        => $"{QueryFilterBindingNamePrefix}{bindingId}";

    private static int ParseFilterIdFromQueryFilterName(string filterName)
    {
        int.TryParse(filterName.Replace(QueryFilterBindingNamePrefix, ""), out int result);
        return result;
    }

    private SqlKata.Query ApplyFilterSet(QueryFilterSet queryFilterSet, QueryCommand queryCommand = null)
    {
        var query = new SqlKata.Query();

        foreach (var queryFilter in queryFilterSet.QueryFilters)
        {
            if (queryFilter.QueryTableColumnId is null)
            {
                ApplySqlFilter(query, queryFilter, queryFilterSet.ConditionalOperator, queryCommand);
            }
            else
            {
                ApplyFilter(query, queryFilter, queryFilterSet.ConditionalOperator, queryCommand);
            }
        }

        foreach (var queryFilterChildSet in queryFilterSet.ChildSets)
        {
            if (queryFilterSet.ConditionalOperator == QueryConditionalOperator.And)
            {
                query.Where(x => ApplyFilterSet(queryFilterChildSet, queryCommand));
            }
            else
            {
                query.OrWhere(x => ApplyFilterSet(queryFilterChildSet, queryCommand));
            }
        }

        return query;
    }

    private void ApplySqlFilter(SqlKata.Query query, QueryFilter queryFilter, QueryConditionalOperator parentCondition,
        QueryCommand queryCommand = null)
    {
        var result = new StringBuilder(queryFilter.CustomSqlCodeTemplate);
        var dynamicParams = new List<dynamic>();
        var matches = new Regex("@([\\w\\d\\.]+)").Matches(queryFilter.CustomSqlCodeTemplate);

        if (matches.Count != 0)
        {
            var querySchemaColumns = _queryTableColumns.Select(_tablesSchema.GetColumn);

            foreach (Match match in matches)
            {
                var paramName = match.Groups[1].Value;
                var relatedSchemaColumn = querySchemaColumns
                    .SingleOrDefault(x => $"{x.ParentTableName}.{x.ColumnName}" == paramName);
                if (relatedSchemaColumn is not null)
                {
                    result.Replace(match.Value, MakeColumnNameUniversalForKata(relatedSchemaColumn.QueryName));
                }
                else if (queryCommand is not null)
                {
                    var queryCommandParameter = queryCommand.Filters.SingleOrDefault(x => x.PropertyName == paramName);

                    if (queryCommandParameter is null)
                        return;

                    var queryCommandParameterValue = queryCommandParameter
                        .GetType().GetProperty("Value").GetValue(queryCommandParameter);

                    if (queryCommandParameterValue is null)
                        return;

                    result.Replace(match.Value, "?");
                    dynamicParams.Add(queryCommandParameterValue);
                }
            }
        }

        if (result.Length != 0)
        {
            var resultStr = result.ToString();

            // It's a hacky patch for SqlKata v2.4 package. The package has an issue -
            // if you add a raw SQL filter into WHERE clause and the filter's expression doesn't contain dynamic
            // parameters then the SQL if build with errors.
            // On the next update of the package we need to check if they fixed it!!!
            if (dynamicParams.Count == 0)
            {
                resultStr = $"({resultStr}) AND (? = 1)";
                dynamicParams.Add(1);
            }

            if (parentCondition == QueryConditionalOperator.And)
                query.WhereRaw(resultStr, dynamicParams);
            else
                query.OrWhereRaw(resultStr, dynamicParams);
        }
    }

    private void ApplyFilter(
        SqlKata.Query query,
        QueryFilter queryFilter,
        QueryConditionalOperator parentCondition,
        QueryCommand queryCommand = null)
    {
        var clauseData = GetWhereClauseData(queryFilter, queryCommand);

        var queryTableColumn = _queryTableColumns.Single(x => x.Id == queryFilter.QueryTableColumnId);
        var schemaColumn = _tablesSchema.GetColumn(queryTableColumn);

        if (schemaColumn.ClrTypeGroup == ClrTypeGroup.Bool)
        {
            if (clauseData.value is null) return;
        }

        if (clauseData.op == QueryRuleCode.Between &&
            (clauseData.from == null || clauseData.to == null) ||
            clauseData.op != QueryRuleCode.Between &&
            (clauseData.value == null || clauseData.value is string s && string.IsNullOrWhiteSpace(s))) return;

        if (parentCondition == QueryConditionalOperator.Or)
        {
            if (clauseData.value is IEnumerable && clauseData.value is not string)
            {
                var method = query.GetType().GetMethods().Single(x => x.Name == nameof(query.OrWhereIn) && x.IsGenericMethod)
                    .MakeGenericMethod(clauseData.value.GetType().GetGenericArguments()[0]);

                if (clauseData.value is IEnumerable<string> stringArrayValue)
                {
                    clauseData.value = stringArrayValue.Select(x => HttpUtility.UrlDecode(x)).ToArray();
                }

                method.Invoke(query, new[] { schemaColumn.QueryName, clauseData.value });
                return;
            }

            switch (clauseData.op)
            {
                case QueryRuleCode.Contains:
                    query.OrWhereContains(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.NotContains:
                    query.OrWhereNotContains(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.StartsWith:
                    query.OrWhereStarts(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.EndsWith:
                    query.OrWhereEnds(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.Between:
                    query.OrWhereBetween(schemaColumn.QueryName, clauseData.from, clauseData.to);
                    break;

                default:
                    query.OrWhere(schemaColumn.QueryName, GetStringOperatorByQueryRuleCode(clauseData.op), clauseData.value);
                    break;
            }
        }
        else
        {
            if (clauseData.value is IEnumerable && clauseData.value is not string)
            {
                var method = query.GetType().GetMethods().Single(x => x.Name == nameof(query.WhereIn) && x.IsGenericMethod)
                    .MakeGenericMethod(clauseData.value.GetType().GetGenericArguments()[0]);

                if (clauseData.value is IEnumerable<string> stringArrayValue)
                {
                    clauseData.value = stringArrayValue.Select(x => HttpUtility.UrlDecode(x)).ToArray();
                }

                method.Invoke(query, new[] { schemaColumn.QueryName, clauseData.value });
                return;
            }

            switch (clauseData.op)
            {
                case QueryRuleCode.Contains:
                    query.WhereContains(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.NotContains:
                    query.WhereNotContains(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.StartsWith:
                    query.WhereStarts(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.EndsWith:
                    query.WhereEnds(schemaColumn.QueryName, clauseData.value);
                    break;

                case QueryRuleCode.Between:
                    query.WhereBetween(schemaColumn.QueryName, clauseData.from, clauseData.to);
                    break;

                default:
                    query.Where(schemaColumn.QueryName, GetStringOperatorByQueryRuleCode(clauseData.op), clauseData.value);
                    break;
            }
        }
    }

    private (object value, object from, object to, QueryRuleCode op) GetWhereClauseData(
        QueryFilter queryFilter,
        QueryCommand command)
    {
        var value = queryFilter.Value;
        var value2 = queryFilter.Value2;
        var op = queryFilter.QueryRule.Code;

        var filter = GetCommandFilterByQueryFilter(queryFilter, command);

        if (filter != null)
        {
            command.Filters.Remove(filter);

            var filterType = filter.GetType();
            var filterTypeInfo = filterType.GetTypeInfo();

            if (filterTypeInfo.IsSubClassOfGeneric(typeof(CountableBetweenFilterBase<>)))
            {
                value = filterType.GetProperty("From")?.GetValue(filter);
                value2 = filterType.GetProperty("To")?.GetValue(filter);
                op = QueryRuleCode.Between;
            }

            if (filterTypeInfo.IsSubClassOfGeneric(typeof(FilterInfoBase<>)))
            {
                value = filterType.GetProperty("Value")?.GetValue(filter);

                var matchModeObj = filterType.GetProperty("MatchMode")?.GetValue(filter);
                if (matchModeObj != null)
                {
                    if (filter is StringFilter)
                    {
                        op = GetOperatorByStringMatchMode((StringFilterMatchMode)matchModeObj);
                    }

                    if (filterTypeInfo.IsSubClassOfGeneric(typeof(CountableFilterBase<>)))
                        op = GetOperatorByCountableMatchMode((CountableFilterMatchMode)matchModeObj);
                }
            }
        }

        if (op == QueryRuleCode.Between)
        {
            if (value == null || value2 == null)
            {
                if (value == null)
                {
                    op = QueryRuleCode.LessOrEqual;
                    value = value2;
                }
                else
                {
                    op = QueryRuleCode.MoreOrEqual;
                }
            }
        }

        if (value is DateTime dateTimeValue && op == QueryRuleCode.Equals)
        {
            value2 = dateTimeValue.AddHours(24).Subtract(TimeSpan.FromMilliseconds(1));
            op = QueryRuleCode.Between;
        }

        return (value, value, value2, op);
    }

    private static FilterInfoBase GetCommandFilterByQueryFilter(QueryFilter queryFilter, QueryCommand command)
    {
        var binding = queryFilter.QueryFilterBindings.SingleOrDefault();

        if (binding is not null)
        {
            switch (binding.BindingType)
            {
                case QueryFilterBindingType.FilterControl:
                    return command?.Filters?.SingleOrDefault(x => x.PropertyName == binding.FilterControl.Name);

                case QueryFilterBindingType.MasterDetailGrid:
                    return command?.Filters?.SingleOrDefault(x => ParseFilterIdFromQueryFilterName(x.PropertyName) == binding.Id);
            }
        }

        return null;
    }

    private QueryRuleCode GetOperatorByStringMatchMode(StringFilterMatchMode matchMode) =>
        matchMode switch
        {
            StringFilterMatchMode.Equals => QueryRuleCode.Equals,
            StringFilterMatchMode.NotEquals => QueryRuleCode.NotEquals,
            StringFilterMatchMode.Contains => QueryRuleCode.Contains,
            StringFilterMatchMode.NotContains => QueryRuleCode.NotContains,
            StringFilterMatchMode.StartsWith => QueryRuleCode.StartsWith,
            StringFilterMatchMode.EndsWith => QueryRuleCode.EndsWith,
            _ => throw new NotSupportedException("The passed string match mode is not supported."),
        };

    private QueryRuleCode GetOperatorByCountableMatchMode(CountableFilterMatchMode matchMode) =>
        matchMode switch
        {
            CountableFilterMatchMode.Equals => QueryRuleCode.Equals,
            CountableFilterMatchMode.NotEquals => QueryRuleCode.NotEquals,
            CountableFilterMatchMode.GreaterThan => QueryRuleCode.More,
            CountableFilterMatchMode.GreaterThanOrEqual => QueryRuleCode.MoreOrEqual,
            CountableFilterMatchMode.LessThan => QueryRuleCode.Less,
            CountableFilterMatchMode.LessThanOrEqual => QueryRuleCode.LessOrEqual,
            _ => throw new NotSupportedException("The passed countable match mode is not supported."),
        };

    private string GetStringOperatorByQueryRuleCode(QueryRuleCode code) =>
        code switch
        {
            QueryRuleCode.Equals => "=",
            QueryRuleCode.NotEquals => "!=",
            QueryRuleCode.More => ">",
            QueryRuleCode.MoreOrEqual => ">=",
            QueryRuleCode.Less => "<",
            QueryRuleCode.LessOrEqual => "<=",
            _ => throw new NotSupportedException("The QueryCodeRule doesn't have an associated string representation.")
        };

    private string MakeColumnNameUniversalForKata(string columnName) =>
        string.Join('.', columnName.Split('.').Select(x => $"[{x}]"));
}