using System.Data.Common;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Model;
using BBF.Reporting.QueryBuilder.Model;
using BBWM.Core.Filters;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbQueryProcessor
{
    IRqbQueryProcessor UseConnectionString(string connectionString);

    string SqlAliasCleanup(string sqlAlias);

    string SqlCodeAliasesCleanup(string sqlCode);

    Task<SqlQueryValidateResult> ValidateSqlCode(string sqlCode, CancellationToken ct = default);

    IEnumerable<(string TableName, string TableAlias)> GetSqlDeclaredTableAliases(string sqlCode);

    IEnumerable<(string Expression, string TableAlias)> GetSqlDerivedTableExpressions(string sqlCode);

    IEnumerable<string> GetQueryVariables(string sqlCode);

    string GetSqlCodeNoVariables(string sqlCode);

    string ProcessSqlCodePaging(string sqlCode, int pageSkip, int pageTake);

    string ProcessSqlCodeSorting(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        string? sortingField, OrderDirection? sortingDirection);

    string ProcessSqlCodeVariables(string sqlCode, QueryVariables queryVariables);

    string ProcessSqlCodeContextVariables(string sqlCode);

    string ProcessSqlCodeDataRowsCount(string sqlCode);

    string ProcessSqlCodeOrganizationBasedFiltering(string sqlCode, IEnumerable<QuerySchemaColumn> columns, QueryFilterMode? filterMode);

    string ProcessSqlCodeAggregations(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        IList<QueryColumnAggregation> aggregations, out IDictionary<string, string[]> aggregationAliases);

    string ProcessSqlCodeUnions(string sqlCode);

    Task<IEnumerable<DbColumn>> ReadSqlQueryColumns(string sqlCode, CancellationToken ct = default);

    Task<IEnumerable<object[]>> ReadSqlQueryData(string sqlCode, CancellationToken ct = default);
}