using System.Data.Common;
using System.Dynamic;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.TableSet.Connectors.DbDoc;
using BBF.Reporting.QueryBuilder.DbModel;
using BBF.Reporting.QueryBuilder.Model;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using BBF.Reporting.QueryBuilder.Interfaces;

namespace BBF.Reporting.QueryBuilder.Services;

/// <summary>
/// This services is an entry point for accessing a query source records based on raw SQL query processing.
/// "Rqb" prefix in group of services stands for Raw Query Builder.
/// </summary>
public class RqbQuerySourceProvider : IRqbQuerySourceProvider
{
    public const string SourceType = "sql";

    private readonly IDbContext _context;
    private readonly IQuerySourceService _querySourceService;
    private readonly IRqbQueryProcessorFactory _sqlBuilderSqlQueryProviderFactory;
    private readonly IDbDocFolderService _dbDocFolderService;
    private readonly IDbSchemaManager _dbSchemaManager;

    public RqbQuerySourceProvider(
        IDbContext context,
        IQuerySourceService querySourceService,
        IRqbQueryProcessorFactory sqlBuilderSqlQueryProviderFactory,
        IDbDocFolderService dbDocFolderService,
        IDbSchemaManager dbSchemaManager)
    {
        _context = context;
        _querySourceService = querySourceService;
        _sqlBuilderSqlQueryProviderFactory = sqlBuilderSqlQueryProviderFactory;
        _dbDocFolderService = dbDocFolderService;
        _dbSchemaManager = dbSchemaManager;
    }

    public async Task<QuerySchema> GetQuerySchema(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);
        var settings = await GetSqlQuerySettings(provider, querySourceId, ct);

        settings.SqlCode = provider.GetSqlCodeNoVariables(settings.SqlCode);

        return new QuerySchema { Columns = await GetSqlQueryColumns(provider, settings.SqlCode, ct) };
    }

    public async Task<IEnumerable<dynamic>> GetQueryDataRows(Guid querySourceId, QueryVariables? queryVariables,
        PagedGridSettings? gridSettings = null, CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);
        var settings = await GetSqlQuerySettings(provider, querySourceId, ct);
        return await GetQueryDataRows(provider, settings, queryVariables, gridSettings, ct);
    }

    public async Task<int> GetQueryDataRowsCount(Guid querySourceId, QueryVariables? queryVariables = null,
        CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);
        var settings = await GetSqlQuerySettings(provider, querySourceId, ct);
        return await GetQueryDataRowsCount(provider, settings, queryVariables, ct);
    }

    public async Task<dynamic> GetQueryDataAggregations(Guid querySourceId, IList<QueryColumnAggregation> aggregations,
        QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);
        var settings = await GetSqlQuerySettings(provider, querySourceId, ct);
        return await GetQueryDataAggregations(provider, settings, aggregations, queryVariables, ct);
    }

    public async Task<IEnumerable<string>> GetQueryVariables(Guid querySourceId, CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);
        var settings = await GetSqlQuerySettings(provider, querySourceId, ct);
        return provider.GetQueryVariables(settings.SqlCode);
    }

    public async Task<SqlQueryValidateResult> ValidateSchemaCompatibility(Guid querySourceId, string oldSqlCode,
        string newSqlCode, CancellationToken ct = default)
    {
        var provider = await GetProvider(querySourceId, ct);

        static bool EqualColumns(QuerySchemaColumn first, QuerySchemaColumn second)
            => first.DataType == second.DataType &&
               string.Equals(first.QueryAlias, second.QueryAlias) &&
               string.Equals(first.TableName, second.TableName) &&
               string.Equals(first.ColumnName, second.ColumnName) &&
               string.Equals(first.BaseTableName, second.BaseTableName) &&
               string.Equals(first.BaseColumnName, second.BaseColumnName);

        var oldQuerySchemaColumns = await GetSqlQueryColumns(provider, oldSqlCode, ct);
        var newQuerySchemaColumns = await GetSqlQueryColumns(provider, newSqlCode, ct);

        var missingColumn = oldQuerySchemaColumns
            .FirstOrDefault(oldColumn => newQuerySchemaColumns
                .All(newColumn => !EqualColumns(oldColumn, newColumn)));

        return missingColumn == null
            ? new SqlQueryValidateResult { Valid = true }
            : new SqlQueryValidateResult
            {
                Valid = false,
                Message = $"Column '{missingColumn.QueryAlias}' of old query schema is missing in new schema."
            };
    }

    public async Task<SqlQueryValidateResult> ValidateSqlCode(int? tableSetId, string sqlCode, CancellationToken ct = default)
        => await (await GetProvider(tableSetId, ct)).ValidateSqlCode(sqlCode, ct);

    public async Task<Guid> ReleaseDraft(Guid querySourceId, CancellationToken ct = default)
    {
        var draftQuery = await _context.Set<SqlQuery>()
                             .FirstOrDefaultAsync(query => query.QuerySourceId == querySourceId, ct)
                         ?? throw new ObjectNotExistsException("Draft SQL query with specified ID doesn't exist.");

        var releasedQueryId = await _querySourceService.ReleaseDraft(querySourceId, ct);
        if (releasedQueryId == querySourceId) return querySourceId;

        var releaseQuery = await _context.Set<SqlQuery>()
                               .FirstOrDefaultAsync(query => query.QuerySourceId == releasedQueryId, ct)
                           ?? throw new ObjectNotExistsException("Released SQL query with specified ID doesn't exist.");

        // Copy edited draft fields to released query.
        releaseQuery.SqlCode = draftQuery.SqlCode;
        releaseQuery.TableSetId = draftQuery.TableSetId;
        releaseQuery.TableSet = draftQuery.TableSet;
        await _context.SaveChangesAsync(ct);

        return releasedQueryId;
    }

    private async Task<SqlQuerySettings> GetSqlQuerySettings(IRqbQueryProcessor provider, Guid querySourceId,
        CancellationToken ct = default)
    {
        var settings = await _context.Set<SqlQuery>()
                           .Where(query => query.QuerySourceId == querySourceId)
                           .Select(query => new SqlQuerySettings { SqlCode = query.SqlCode, FilterMode = query.QuerySource.FilterMode })
                           .FirstOrDefaultAsync(ct)
                       ?? throw new ObjectNotExistsException($"SQL code for query source ID '{querySourceId}' not found.");

        settings.SqlCode = provider.SqlCodeAliasesCleanup(settings.SqlCode);
        settings.SqlCode = provider.ProcessSqlCodeUnions(settings.SqlCode);
        return settings;
    }

    private async Task<IEnumerable<QuerySchemaColumn>> GetSqlQueryColumns(IRqbQueryProcessor provider,
        string sqlCode, CancellationToken ct = default)
    {
        // Read query column schema.
        var dbColumns = await provider.ReadSqlQueryColumns(sqlCode, ct);

        // Declare query schema columns.
        var querySchemaColumns =
            (from dbColumn in dbColumns
             let dataType = DbTypeToReportingColumnType(dbColumn)
             let isAliased = dbColumn.IsAliased == true
             let columnName = dbColumn.ColumnName
             let baseTableName = dbColumn.BaseTableName
             let baseColumnName = dbColumn.BaseColumnName ?? dbColumn.ColumnName
             select new QuerySchemaColumn
             {
                 IsAliased = isAliased,
                 ColumnName = columnName,
                 TableName = baseTableName,
                 BaseTableName = baseTableName,
                 BaseColumnName = baseColumnName,
                 DataType = dataType
             }).ToList();

        // Get true table definitions from query (schema reader doesn't return information about table aliases).
        var tables = provider.GetSqlDeclaredTableAliases(sqlCode);

        // Fix columns table names for columns that belong to aliased tables.
        foreach (var table in tables)
        {
            // Filter the query schema columns that belong to this table.
            // A query schema column belong to this table if it has this table name as base table name.
            var columns = querySchemaColumns.Where(column =>
                string.Equals(column.BaseTableName, table.TableName, StringComparison.OrdinalIgnoreCase));

            foreach (var column in columns) column.TableName = table.TableAlias;
        }

        // Get derived table definitions from query (schema reader doesn't return information about table aliases).
        var derivedTables = provider.GetSqlDerivedTableExpressions(sqlCode);

        // Fix columns table names for columns that belong to derived tables.
        foreach (var derivedTable in derivedTables)
        {
            // Get the db columns that belong to this derived table.
            var derivedColumns = await provider.ReadSqlQueryColumns(derivedTable.Expression, ct);

            // Filter the query schema columns that belong to this table.
            // A query schema column belong to this table if there is a db column in this table
            // that has the same base column name and base table name.
            var columns = querySchemaColumns.Where(column => derivedColumns
                .Any(derivedColumn => column.BaseTableName == derivedColumn.BaseTableName &&
                                      column.BaseColumnName == derivedColumn.BaseColumnName));

            foreach (var column in columns) column.TableName = derivedTable.TableAlias;
        }

        // Declare query aliases.
        foreach (var column in querySchemaColumns)
        {
            if (!column.IsAliased && column.TableName is { Length: > 0 })
            {
                var tableName = provider.SqlAliasCleanup(column.TableName);
                var columnName = provider.SqlAliasCleanup(column.ColumnName);
                column.QueryAlias = $"{tableName}.{columnName}";
            }
            else column.QueryAlias = provider.SqlAliasCleanup(column.ColumnName);
        }

        return querySchemaColumns;
    }

    private async Task<IEnumerable<dynamic>> GetSqlQueryData(IRqbQueryProcessor provider, string sqlCode,
        IEnumerable<QuerySchemaColumn>? columns = null, CancellationToken ct = default)
    {
        var dbData = await provider.ReadSqlQueryData(sqlCode, ct);
        var querySchemaColumns = (columns ?? await GetSqlQueryColumns(provider, sqlCode, ct)).ToList();

        return dbData.Select(data =>
        {
            dynamic rowData = new ExpandoObject();
            IDictionary<string, object?> underlyingObject = rowData;

            for (var i = 0; i < querySchemaColumns.Count; i++)
            {
                var queryColumn = querySchemaColumns[i];
                underlyingObject.Add(queryColumn.QueryAlias, data[i] is DBNull ? null : data[i]);
            }

            return rowData;
        });
    }

    private async Task<IRqbQueryProcessor> GetProvider(int? tableSetId, CancellationToken ct = default)
    {
        var tableSet = await _context.Set<TableSet.DbModel.TableSet>().AsNoTracking()
            .FirstOrDefaultAsync(set => set.Id == tableSetId, ct);

        var dbSource = await GetQueryDbSource(tableSet, ct);
        return _sqlBuilderSqlQueryProviderFactory.GetSqlQueryProvider(dbSource.DatabaseType)
                   ?.UseConnectionString(dbSource.ConnectionString)
               ?? throw new BusinessException("Cannot find SQL query provider.");
    }

    private async Task<IRqbQueryProcessor> GetProvider(Guid querySourceId, CancellationToken ct = default)
    {
        var tableSet = await _context.Set<SqlQuery>().AsNoTracking()
            .Where(query => query.QuerySourceId == querySourceId)
            .Select(query => query.TableSet)
            .FirstOrDefaultAsync(ct);

        var dbSource = await GetQueryDbSource(tableSet, ct);

        return _sqlBuilderSqlQueryProviderFactory.GetSqlQueryProvider(dbSource.DatabaseType)
                   ?.UseConnectionString(dbSource.ConnectionString)
               ?? throw new BusinessException("Cannot find SQL query provider.");
    }

    private async Task<DatabaseSource> GetQueryDbSource(TableSet.DbModel.TableSet? tableSet, CancellationToken ct = default)
    {
        switch (tableSet?.FolderSourceCode)
        {
            case DbDocConnector.SourceCode:
                {
                    if (tableSet.FolderId is null)
                        throw new ObjectNotExistsException($"Folder for table set ID '{tableSet.Id}' is not set.");

                    var dbSourceId = await _dbDocFolderService.GetFolderDatabaseSourceId(Guid.Parse(tableSet.FolderId), ct)
                                   ?? throw new ObjectNotExistsException($"Database for this folder is not found. " +
                                                                         $"Note: current reporting version supports only DB " +
                                                                         $"Documenting folders created by connected " +
                                                                         $"database. Custom folders are not supported yet.");

                    return _dbSchemaManager.GetDbSchema(dbSourceId).DatabaseSource;
                }

            // In this version, the Forms module only uses the main DB.
            default:
                return _dbSchemaManager.GetMainDbSchema().DatabaseSource;
        }
    }

    private static DataType DbTypeToReportingColumnType(DbColumn dbColumn)
    {
        var dataType = DataType.Other;

        if (dbColumn.DataType == typeof(int))
            dataType = DataType.Numeric;
        else if (dbColumn.DataType == typeof(Date)
                 || dbColumn.DataType == typeof(DateTime)
                 || dbColumn.DataType == typeof(DateTimeOffset))
            dataType = DataType.Date;
        else if (dbColumn.DataType == typeof(string))
            dataType = DataType.String;
        else if (dbColumn.DataType == typeof(bool))
            dataType = DataType.Bool;
        return dataType;
    }

    private async Task<IEnumerable<dynamic>> GetQueryDataRows(IRqbQueryProcessor provider,
        SqlQuerySettings settings, QueryVariables? queryVariables, PagedGridSettings? gridSettings = null,
        CancellationToken ct = default)
    {
        var querySchemaColumns = (await GetSqlQueryColumns(provider, settings.SqlCode, ct)).ToList();
        if (querySchemaColumns is not { Count: > 0 })
            throw new BusinessException("SQL query has no declared columns.");

        var skip = gridSettings?.Skip ?? 0;
        var take = gridSettings?.Take ?? 100;
        var sortingField = gridSettings?.SortingField;
        var sortingDirection = gridSettings?.SortingDirection;

        settings.SqlCode = provider.ProcessSqlCodeSorting(settings.SqlCode, querySchemaColumns, sortingField, sortingDirection);
        settings.SqlCode = provider.ProcessSqlCodePaging(settings.SqlCode, skip, take);
        settings.SqlCode = provider.ProcessSqlCodeVariables(settings.SqlCode, queryVariables ?? new QueryVariables());
        settings.SqlCode = provider.ProcessSqlCodeContextVariables(settings.SqlCode);
        settings.SqlCode = provider.ProcessSqlCodeOrganizationBasedFiltering(settings.SqlCode, querySchemaColumns, settings.FilterMode);

        return await GetSqlQueryData(provider, settings.SqlCode, querySchemaColumns, ct);
    }

    private async Task<int> GetQueryDataRowsCount(IRqbQueryProcessor provider, SqlQuerySettings settings,
        QueryVariables? queryVariables = null, CancellationToken ct = default)
    {
        var querySchemaColumns = (await GetSqlQueryColumns(provider, settings.SqlCode, ct)).ToList();

        settings.SqlCode = provider.ProcessSqlCodeVariables(settings.SqlCode, queryVariables ?? new QueryVariables());
        settings.SqlCode = provider.ProcessSqlCodeContextVariables(settings.SqlCode);
        settings.SqlCode = provider.ProcessSqlCodeOrganizationBasedFiltering(settings.SqlCode, querySchemaColumns, settings.FilterMode);
        settings.SqlCode = provider.ProcessSqlCodeDataRowsCount(settings.SqlCode);

        var data = await GetSqlQueryData(provider, settings.SqlCode, ct: ct);
        var result = data.FirstOrDefault() as ExpandoObject;
        var value = result?.FirstOrDefault().Value;
        return value == null ? 0 : int.Parse(value.ToString()!);
    }

    private async Task<dynamic> GetQueryDataAggregations(IRqbQueryProcessor provider, SqlQuerySettings settings,
        IList<QueryColumnAggregation> aggregations, QueryVariables? queryVariables = null,
        CancellationToken ct = default)
    {
        var querySchemaColumns = (await GetSqlQueryColumns(provider, settings.SqlCode, ct)).ToList();

        settings.SqlCode = provider.ProcessSqlCodeVariables(settings.SqlCode, queryVariables ?? new QueryVariables());
        settings.SqlCode = provider.ProcessSqlCodeContextVariables(settings.SqlCode);
        settings.SqlCode = provider.ProcessSqlCodeOrganizationBasedFiltering(settings.SqlCode, querySchemaColumns, settings.FilterMode);
        settings.SqlCode = provider.ProcessSqlCodeAggregations(settings.SqlCode, querySchemaColumns, aggregations,
            out var aggregationAliases);

        // Get the result corresponding to this new query.
        var data = await GetSqlQueryData(provider, settings.SqlCode, ct: ct);
        var value = data.FirstOrDefault() as IDictionary<string, object?>;

        // Group the result values by original column query alias, to make them easier to handle.
        var result = aggregations.ToDictionary(aggregation => aggregation.QueryAlias,
            aggregation => aggregationAliases.TryGetValue(aggregation.QueryAlias, out var aliases)
                ? aliases.Select(alias => value != null ? value[alias] : string.Empty)
                : Array.Empty<dynamic>());

        return result;
    }
}