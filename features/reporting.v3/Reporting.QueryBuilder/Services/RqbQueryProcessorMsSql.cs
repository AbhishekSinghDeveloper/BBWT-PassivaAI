using System.Data;
using System.Data.Common;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Model;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBF.Reporting.QueryBuilder.Model;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using Microsoft.Data.SqlClient;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryProcessorMsSql : IRqbQueryProcessorMsSql
{
    public const DatabaseType DbType = DatabaseType.MsSql;

    private readonly IRqbQueryProcessorDefault _commonSqlQueryProvider;
    private string? _connectionString;

    public RqbQueryProcessorMsSql(IRqbQueryProcessorDefault commonSqlQueryProvider)
    {
        _commonSqlQueryProvider = commonSqlQueryProvider;
    }

    public IRqbQueryProcessor UseConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public string SqlAliasCleanup(string sqlAlias)
        => _commonSqlQueryProvider.SqlAliasCleanup(sqlAlias);

    public string SqlCodeAliasesCleanup(string sqlCode)
        => _commonSqlQueryProvider.SqlCodeAliasesCleanup(sqlCode);

    public async Task<SqlQueryValidateResult> ValidateSqlCode(string sqlCode, CancellationToken ct = default)
    {
        try
        {
            var validation = _commonSqlQueryProvider.ValidateSqlCode(sqlCode);
            if (!validation.Valid) return validation;

            await using var connection = new SqlConnection(GetConnectionString());

            sqlCode = _commonSqlQueryProvider.SqlCodeCleanup(sqlCode);
            sqlCode = GetSqlCodeNoVariables(sqlCode);

            await using var command = new SqlCommand(sqlCode, connection);
            await connection.OpenAsync(ct);
            await command.ExecuteReaderAsync(ct);
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            return new SqlQueryValidateResult
            {
                Valid = false,
                Message = ex.Message
            };
        }

        return new SqlQueryValidateResult { Valid = true };
    }

    public IEnumerable<(string TableName, string TableAlias)> GetSqlDeclaredTableAliases(string sqlCode)
        => _commonSqlQueryProvider.GetSqlDeclaredTableAliases(sqlCode);

    public IEnumerable<(string Expression, string TableAlias)> GetSqlDerivedTableExpressions(string sqlCode)
        => _commonSqlQueryProvider.GetSqlDerivedTableExpressions(sqlCode);

    public IEnumerable<string> GetQueryVariables(string sqlCode)
        => _commonSqlQueryProvider.GetQueryVariables(sqlCode);

    public string GetSqlCodeNoVariables(string sqlCode)
        => _commonSqlQueryProvider.GetSqlCodeNoVariables(sqlCode);

    public string ProcessSqlCodePaging(string sqlCode, int pageSkip, int pageTake)
        => $"{sqlCode} OFFSET {pageSkip} ROWS FETCH NEXT {pageTake} ROWS ONLY";

    public string ProcessSqlCodeSorting(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        string? sortingField, OrderDirection? sortingDirection)
        => _commonSqlQueryProvider.ProcessSqlCodeSorting(sqlCode, columns, sortingField, sortingDirection);

    public string ProcessSqlCodeVariables(string sqlCode, QueryVariables queryVariables)
        => _commonSqlQueryProvider.ProcessSqlCodeVariables(sqlCode, queryVariables);

    public string ProcessSqlCodeContextVariables(string sqlCode)
        => _commonSqlQueryProvider.ProcessSqlCodeContextVariables(sqlCode);

    public string ProcessSqlCodeDataRowsCount(string sqlCode)
        => _commonSqlQueryProvider.ProcessSqlCodeDataRowsCount(sqlCode);

    public string ProcessSqlCodeOrganizationBasedFiltering(string sqlCode, IEnumerable<QuerySchemaColumn> columns, QueryFilterMode? filterMode)
        => _commonSqlQueryProvider.ProcessSqlCodeOrganizationBasedFiltering(sqlCode, columns, filterMode);

    public string ProcessSqlCodeAggregations(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        IList<QueryColumnAggregation> aggregations, out IDictionary<string, string[]> aggregationAliases)
        => _commonSqlQueryProvider.ProcessSqlCodeAggregations(sqlCode, columns, aggregations, out aggregationAliases);

    public string ProcessSqlCodeUnions(string sqlCode)
        => _commonSqlQueryProvider.ProcessSqlCodeUnions(sqlCode);

    public async Task<IEnumerable<DbColumn>> ReadSqlQueryColumns(string sqlCode, CancellationToken ct = default)
    {
        // Try to use only the query specification instead of the whole sql code to read the schema.
        sqlCode = _commonSqlQueryProvider.GetSqlCodeSelectStatement(sqlCode);

        // Read the column schema corresponding to this query from the database.
        const CommandBehavior behavior = CommandBehavior.SchemaOnly;
        await using var connection = new SqlConnection(GetConnectionString());
        await using var command = new SqlCommand(sqlCode, connection);
        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(behavior, ct);

        return await reader.GetColumnSchemaAsync(ct);
    }

    public async Task<IEnumerable<object[]>> ReadSqlQueryData(string sqlCode, CancellationToken ct = default)
    {
        // Read the data corresponding to this query from the database.
        const CommandBehavior behavior = CommandBehavior.Default;
        await using var connection = new SqlConnection(GetConnectionString());
        await using var command = new SqlCommand(sqlCode, connection);
        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(behavior, ct);

        var dbData = new List<object[]>();
        while (await reader.ReadAsync(ct))
        {
            var row = new object[reader.FieldCount];
            reader.GetValues(row);
            dbData.Add(row);
        }

        return dbData;
    }

    private string GetConnectionString()
        => _connectionString
           ?? throw new BusinessException("Connection string is not set for MS SQL query provider.");
}