using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Core.Model.Variables;
using BBF.Reporting.QueryBuilder.Enums;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;
using BBF.Reporting.QueryBuilder.Model;
using BBF.Reporting.QueryBuilder.Model.ParserModels;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Model;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryProcessorMySql : IRqbQueryProcessorMySql
{
    public const DatabaseType DbType = DatabaseType.MySql;

    private const char VarPrefix = '#';
    private const char ContextVarPrefix = '@';

    private string? _connectionString;

    private readonly IDbContext _context;
    private readonly IRqbQueryMySqlParser _parser;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IRqbQueryGraphService _tableGraphService;
    private readonly IContextVariableService _contextVariableService;

    public RqbQueryProcessorMySql(
        IDbContext context,
        IRqbQueryMySqlParser parser,
        ILoggedUserService loggedUserService,
        IRqbQueryGraphService tableGraphService,
        IContextVariableService contextVariableService)
    {
        _parser = parser;
        _context = context;
        _loggedUserService = loggedUserService;
        _tableGraphService = tableGraphService;
        _contextVariableService = contextVariableService;
    }

    public IRqbQueryProcessor UseConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public string SqlAliasCleanup(string sqlAlias)
        => _parser.SqlAliasCleanup(sqlAlias);

    public string SqlCodeAliasesCleanup(string sqlCode)
        => _parser.SqlCodeAliasesCleanup(sqlCode);

    public async Task<SqlQueryValidateResult> ValidateSqlCode(string sqlCode, CancellationToken ct = default)
    {
        try
        {
            sqlCode = _parser.SqlCodeCleanup(sqlCode);

            if (sqlCode is not { Length: > 0 })
                return new SqlQueryValidateResult
                {
                    Valid = false,
                    Message = "SQL code cannot be empty."
                };

            if (_parser.GetSelectClause(sqlCode) == null)
                return new SqlQueryValidateResult
                {
                    Valid = false,
                    Message = "Only SELECT queries are supported."
                };

            sqlCode = GetSqlCodeNoVariables(sqlCode);

            await using var connection = new MySqlConnection(GetConnectionString());
            await using var command = new MySqlCommand(sqlCode, connection);
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
        => _parser.GetTableReferences(sqlCode)
            .Where(table => table is { Name.Length: > 0, Alias.Length: > 0 })
            .Select(table => (TableName: table.Name, TableAlias: table.Alias))!;

    public IEnumerable<(string Expression, string TableAlias)> GetSqlDerivedTableExpressions(string sqlCode)
        => _parser.GetDerivedTableExpressions(sqlCode)
            .Where(table => table is { QueryExpression.Sql.Length: > 0, Alias.Length: > 0 })
            .Select(table => (Expression: table.QueryExpression!.Sql, TableAlias: table.Alias))!;

    public IEnumerable<string> GetQueryVariables(string sqlCode)
        => _parser.GetSqlParserObjectsOfType<EmittedVariableReference>(sqlCode)
            .Select(variable => variable.Name);

    public string GetSqlCodeSelectStatement(string sqlCode)
    {
        var selectStatement = _parser.GetSelectStatement(sqlCode);
        var querySpecification = selectStatement?.QueryExpression as QuerySpecification;
        var selectClause = querySpecification?.SelectClause;
        var fromClause = querySpecification?.FromClause;
        var groupByClause = querySpecification?.GroupByClause;

        // If there is no SELECT or FROM clause, return sql code without modifications.
        if (string.IsNullOrEmpty(selectClause?.Sql) || string.IsNullOrEmpty(fromClause?.Sql)) return sqlCode;

        sqlCode = $"{selectClause.Sql} {fromClause.Sql}";

        // If GROUP BY clause is declared, concatenate it also at the end of the query specification.
        if (!string.IsNullOrEmpty(groupByClause?.Sql)) sqlCode += $" {groupByClause.Sql}";

        return sqlCode;
    }

    public string GetSqlCodeNoVariables(string sqlCode)
        => ProcessSqlCodeContextVariables(ProcessSqlCodeVariables(sqlCode, new QueryVariables()));

    // Interesting optimization to consider: https://planetscale.com/blog/mysql-pagination
    // "Deferred joins for faster offset/limit pagination" section.
    // The deferred join technique is an optimization solution that enables more efficient pagination.
    // It performs the pagination on a subset of the data instead of the entire table. This subset is
    // generated by a subquery, which is joined with the original table later. The technique is called
    // "deferred" because the join operation is postponed until after the pagination is done.
    //     SELECT * FROM people
    //          INNER JOIN(
    //            -- Paginate the narrow subquery instead of the entire table
    //              SELECT id FROM people ORDER BY first_name, id LIMIT 10 OFFSET 450000
    //          ) AS tmp USING(id)
    //      ORDER BY first_name, id
    public string ProcessSqlCodePaging(string sqlCode, int pageSkip, int pageTake)
    {
        var selectStatement = _parser.GetSelectStatement(sqlCode);
        if (selectStatement is null) return sqlCode;

        var limitClause = selectStatement.LimitClause;
        var lastClause = selectStatement.Children.LastOrDefault();

        if (limitClause is not null)
        {
            if (int.TryParse(limitClause.Limit?.Value, out var limit))
                pageTake = Math.Min(pageTake, limit);

            if (int.TryParse(limitClause.Offset?.Value, out var offset))
                pageSkip = Math.Min(pageSkip, offset);

            var replacement = $"LIMIT {pageTake} OFFSET {pageSkip}";

            return _parser.ReplaceSqlParserObject(sqlCode, limitClause, replacement);
        }

        var insertion = $"LIMIT {pageTake} OFFSET {pageSkip}";

        return _parser.InsertAfterSqlParserObject(sqlCode, lastClause, insertion);
    }

    public string ProcessSqlCodeSorting(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        string? sortingField, OrderDirection? sortingDirection)
    {
        var selectStatement = _parser.GetSelectStatement(sqlCode);
        if (selectStatement is null) return sqlCode;

        var limitClause = selectStatement.LimitClause;
        var orderByClause = selectStatement.OrderByClause;
        var lastClause = selectStatement.Children.LastOrDefault();

        var direction = (sortingDirection ?? OrderDirection.Asc) == OrderDirection.Asc ? "ASC" : "DESC";

        if (orderByClause is not null)
        {
            if (string.IsNullOrEmpty(sortingField)) return sqlCode;

            var replacement = $"ORDER BY {sortingField} {direction}";

            return _parser.ReplaceSqlParserObject(sqlCode, orderByClause, replacement);
        }

        if (string.IsNullOrEmpty(sortingField))
            sortingField = columns.First().QueryAlias;

        var insertion = $"ORDER BY {sortingField} {direction}";

        return limitClause is null
            ? _parser.InsertAfterSqlParserObject(sqlCode, lastClause, insertion)
            : _parser.InsertBeforeSqlParserObject(sqlCode, limitClause, insertion);
    }

    public string ProcessSqlCodeVariables(string sqlCode, QueryVariables queryVariables)
    {
        // Get instanced variables.
        var instancedVariables = queryVariables.Variables.Where(variable => !variable.Empty).ToList();

        // Replace all instanced variables by its corresponding value.
        sqlCode = instancedVariables.Aggregate(sqlCode, (current, variable) =>
        {
            var replacement = GetVariableValueToEmbed(variable);
            var pattern = $@"(?<!\w){VarPrefix}{variable.Name}(?!\w)";
            return Regex.Replace(current, pattern, replacement);
        });

        // Get null literals that belong to boolean expressions.
        var nullWhereExpressions = _parser.GetSqlParserObjectsOfType<NullLiteral>(sqlCode)
            .Where(nullLiteral => nullLiteral.Parent is WhereExpression or ListExpression { Parent: WhereExpression })
            .Select(nullLiteral => nullLiteral.Parent as WhereExpression ?? nullLiteral.Parent!.Parent as WhereExpression)
            .Reverse()!.ToList<WhereExpression>();

        // Fix all boolean operations using null values.
        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        foreach (var nullWhereExpression in nullWhereExpressions)
        {
            switch (nullWhereExpression)
            {
                // If expression is an "IN" expression.
                case InExpression inExpression:
                {
                    // Extract expression members.
                    var expression = inExpression.Sql;
                    var operand = inExpression.Left?.Sql;
                    var @operator = inExpression.Negated ? "IS NOT" : "IS";

                    // If some of them is missing, continue.
                    if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(operand)) continue;

                    // Otherwise replace it with a new expression with null check.
                    var replacement = $"({expression} OR {operand} {@operator} NULL)";
                    sqlCode = _parser.ReplaceSqlParserObject(sqlCode, inExpression, replacement);
                    break;
                }
                // If expression is a comparison expression.
                case ComparisonExpression comparisonExpression:
                {
                    // Extract expression members.
                    var expression = comparisonExpression.Sql;
                    var operand = comparisonExpression.Left is NullLiteral
                        ? comparisonExpression.Left.Sql
                        : comparisonExpression.Right.Sql;
                    var @operator = comparisonExpression.Operation switch
                    {
                        ComparisonOperation.Equals => "IS",
                        ComparisonOperation.LessThanOrEqual => "IS",
                        ComparisonOperation.GreaterThanOrEqual => "IS",
                        ComparisonOperation.NotEqual => "IS NOT",
                        _ => null
                    };

                    // If some of them is missing, continue.
                    if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(operand) || string.IsNullOrEmpty(@operator)) continue;

                    // Otherwise replace it with a new expression with null check.
                    var replacement = $"({expression} OR {operand} {@operator} NULL)";
                    sqlCode = _parser.ReplaceSqlParserObject(sqlCode, comparisonExpression, replacement);
                    break;
                }
            }
        }

        // Get clean variables.
        var cleanVariables = queryVariables.Variables
            .Where(variable => variable is { Empty: true, BehaviorOnEmpty: EmittedVariableBehavior.Clean }).ToList();

        // Replace all clean variables by null.
        sqlCode = cleanVariables.Aggregate(sqlCode, (current, variable) =>
        {
            var pattern = $@"(?<!\w){VarPrefix}{variable.Name}(?!\w)";
            return Regex.Replace(current, pattern, "(null)");
        });

        // Get all declared variables.
        var nonEmittedVariables = _parser.GetSqlParserObjectsOfType<EmittedVariableReference>(sqlCode).ToList();

        // Get remaining variable sql code objects that belong to boolean expressions.
        var variableWhereExpressions = nonEmittedVariables
            .Where(variable => variable.Parent is WhereExpression or ListExpression { Parent: WhereExpression })
            .Select(variable => variable.Parent as WhereExpression ?? variable.Parent!.Parent as WhereExpression)
            .Reverse()!.ToList<WhereExpression>();

        // Fix all remaining boolean expression using empty variables to ignore them.
        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        foreach (var variableWhereExpression in variableWhereExpressions)
        {
            var expression = variableWhereExpression.Sql;
            var replacement = $"({expression} OR 1 = 1)";
            sqlCode = _parser.ReplaceSqlParserObject(sqlCode, variableWhereExpression, replacement);
        }

        // Replace all unused variables by null, to force all operations using them to return unknown.
        sqlCode = nonEmittedVariables.Aggregate(sqlCode, (current, variable) =>
        {
            var pattern = $@"(?<!\w){VarPrefix}{variable.Name}(?!\w)";
            return Regex.Replace(current, pattern, "null");
        });

        return sqlCode;
    }

    private static string GetVariableValueToEmbed(EmittedVariable variable)
    {
        return variable switch
        {
            EmittedStringVariable stringVariable => GetStringValue(stringVariable.Value),
            EmittedStringArrayVariable stringArrayVariable => GetStringValueList(stringArrayVariable.Value),
            EmittedNumberVariable numberVariable => GetStringValue(numberVariable.Value),
            EmittedNumberArrayVariable numberArrayVariable => GetStringValueList(numberArrayVariable.Value),
            EmittedDateVariable dateVariable => GetStringValue(dateVariable.Value),
            EmittedDateArrayVariable dateArrayVariable => GetStringValueList(dateArrayVariable.Value),
            EmittedBooleanVariable booleanVariable => GetStringValue(booleanVariable.Value),
            _ => "(null)"
        };

        // Method to convert object value in a list of string values, if possible.
        string GetStringValueList(object? rawValue)
        {
            if (rawValue is not IEnumerable enumerable) return "(null)";
            var values = enumerable.Cast<object>().Select(GetStringValue).ToList();
            return values.Count > 0 ? $"({string.Join(", ", values)})" : "(null)";
        }

        // Method to get the actual value from the raw value.
        string GetStringValue(object? rawValue)
        {
            return rawValue switch
            {
                null => "null",
                bool booleanValue => booleanValue ? "1" : "0",
                DateTime dateTime => $"'{dateTime.Year}-{dateTime.Month}-{dateTime.Day}'",
                decimal numericValue => numericValue.ToString(CultureInfo.InvariantCulture),
                _ when rawValue.ToString() is { } stringValue => $"'{stringValue.Replace("'", "''")}'",
                _ => "(null)"
            };
        }
    }

    public string ProcessSqlCodeContextVariables(string sqlCode)
    {
        var contextVariables = _parser.GetSqlParserObjectsOfType<ContextVariableReference>(sqlCode).ToList();

        return contextVariables.Aggregate(sqlCode, (current, variable) =>
            current.Replace(ContextVarPrefix + variable.Name, _contextVariableService.GetVariableValue(variable.Name) ?? "(null)"));
    }

    public string ProcessSqlCodeDataRowsCount(string sqlCode)
    {
        // Remove ORDER BY sentence to remove using of aliases outside the main query.
        var orderByClause = _parser.GetOrderByClause(sqlCode);
        sqlCode = _parser.RemoveSqlParserObject(sqlCode, orderByClause);

        var limitClause = _parser.GetLimitClause(sqlCode);
        var groupByClause = _parser.GetGroupByClause(sqlCode);

        // If there is no group by clause, build a new query substituting the original SELECT clause.
        if (groupByClause == null && limitClause == null)
        {
            var selectClause = _parser.GetSelectClause(sqlCode)
                               ?? throw new BusinessException("Cannot find select clause in given sql query.");

            // If DISTINCT qualifier was applied, apply count over different combinations of the columns selected in the original query.
            // Otherwise, just count all records.
            var replacement = selectClause.Distinct
                ? $"SELECT COUNT(DISTINCT {string.Join(", ", selectClause.SelectExpressions.Select(expression => expression.Sql))}) AS _count"
                : "SELECT COUNT(1) AS _count";

            // Replace the old SELECT clause with the new one.
            sqlCode = _parser.ReplaceSqlParserObject(sqlCode, selectClause, replacement);
        }
        // Otherwise, wrap the original as subquery of new count query.
        else sqlCode = $"SELECT COUNT(1) FROM ({sqlCode}) AS TableGroups";

        return sqlCode;
    }

    public string ProcessSqlCodeOrganizationBasedFiltering(string sqlCode, IEnumerable<QuerySchemaColumn> columns, QueryFilterMode? filterMode)
    {
        // Don't filter if user is system admin.
        if (_loggedUserService.IsSystemAdmin()) return sqlCode;

        // Get source tables associated with this query.
        var querySchemaColumns = columns.ToList();
        var sourceTableNames = querySchemaColumns
            .Select(column => column.BaseTableName)
            .Distinct().ToList();
        if (!sourceTableNames.Any()) throw new BusinessException("There is no tables associated to this query.");

        var destinyTableName = _context.Model.FindEntityType(typeof(Organization))?.GetTableName()
                               ?? throw new BusinessException("Cannot find Organization table.");

        var destinyPrimaryKey = _context.Model.FindEntityType(typeof(Organization))?.FindPrimaryKey()?.Properties[0].GetColumnName()
                                ?? throw new BusinessException("Cannot find Organization table primary key.");

        // Get organization table alias if it's declared in the query.
        var destinyTableAlias = _parser.GetTableExpression(sqlCode, destinyTableName) switch
        {
            // If it's an aliased table reference declaration, substitute its base name with the alias, for the join clause.
            TableReference { Alias: not null } tableReference => tableReference.Alias,
            // If it's into a derived table declaration, substitute its base name with the alias, for the join clause.
            DerivedTableExpression { Alias: not null } derivedTable => derivedTable.Alias,
            // Otherwise, keep the same table name.
            _ => destinyTableName
        };

        // Exclude all relations from users table to any table different from
        // user-organizations or organizations table (exclusively) depending on query filter mode.
        var exclusions = GetExcludedRelationsByFilterMode(filterMode);

        // Get the shortest path from source tables to organization table.
        // If there is no path, return the same source code.
        var path = _tableGraphService.GetShortestPath(sourceTableNames, destinyTableName, exclusions)?.ToList();
        if (path == null) return sqlCode;

        // If the path is not empty.
        if (path.Count > 0)
        {
            // Get the first table of the path (source table).
            var source = path.First().StartTableColumn;

            // Find the declaration in the query of the first table in the path (source table).
            var tableDeclaration = _parser.GetTableExpression(sqlCode, source.ParentTableName);

            source.ParentTableName = tableDeclaration switch
            {
                // If table declaration it's not found, throw error.
                null => throw new BusinessException("Initial table is not declared correctly."),
                // If it's an aliased table reference declaration, substitute its base name with the alias, for the join clause.
                TableReference { Alias: not null } tableReference => tableReference.Alias,
                // If it's into a derived table declaration, substitute its base name with the alias, for the join clause.
                DerivedTableExpression { Alias: not null } derivedTable => derivedTable.Alias,
                // Otherwise, keep the same table name.
                _ => source.ParentTableName
            };

            // Build the JOIN sentences.
            var joinSentences = path.Select(relation =>
            {
                var startColumnName = relation.StartTableColumn.ColumnName;
                var startTableName = relation.StartTableColumn.ParentTableName;
                var endColumnName = relation.EndTableColumn.ColumnName;
                var endTableName = relation.EndTableColumn.ParentTableName;
                if (startColumnName == null || startTableName == null ||
                    endColumnName == null || endTableName == null) return "";

                return $"JOIN {endTableName} ON {startTableName}.{startColumnName} = {endTableName}.{endColumnName}";
            });

            // Build the JOIN clause and introduce it in the query.
            var joinInsertion = $" {string.Join(" ", joinSentences)}";
            sqlCode = _parser.InsertAfterSqlParserObject(sqlCode, tableDeclaration, joinInsertion);
        }

        // Get organization ids corresponding to this user.
        var organizationsIds = GetUserOrganizationIdsByFilterMode(filterMode).ToList();

        var whereClause = _parser.GetWhereClause(sqlCode);
        // If this user has no organisations, block the user's access to all records.
        if (organizationsIds.Count == 0)
        {
            const string blockingClause = " WHERE 0 = 1";
            // Place the blocking clause after the FROM clause if there is no WHERE clause in the query.
            // Set the blocking clause as a replacement of the WHERE clause otherwise.
            if (whereClause == null)
            {
                var fromClause = _parser.GetFromClause(sqlCode);
                sqlCode = _parser.InsertAfterSqlParserObject(sqlCode, fromClause, blockingClause);
            }
            else _parser.ReplaceSqlParserObject(sqlCode, whereClause, blockingClause);
        }
        // Otherwise, build the new filtering clause and introduce it in the query.
        // Build the new filtering clause as a WHERE clause if there is no WHERE clause in the query.
        else if (whereClause == null)
        {
            var fromClause = _parser.GetFromClause(sqlCode);
            var filter = $"({string.Join(", ", organizationsIds)})";
            var whereInsertion = $" WHERE {destinyTableAlias}.{destinyPrimaryKey} IN {filter}";
            sqlCode = _parser.InsertAfterSqlParserObject(sqlCode, fromClause, whereInsertion);
        }
        // Otherwise, set it as appendix of the existing WHERE clause.
        else
        {
            var whereExpression = whereClause.Sql;
            var filter = $"({string.Join(", ", organizationsIds)})";
            var whereReplacement = $"WHERE ({whereExpression}) AND {destinyTableAlias}.{destinyPrimaryKey} IN {filter}";
            sqlCode = _parser.ReplaceSqlParserObject(sqlCode, whereClause, whereReplacement);
        }

        // Make column declaration explicit in select clause, to avoid ambiguity with tables used for filtering.
        var columnsReferences = _parser.GetSqlParserObjectsOfType<ColumnReference>(sqlCode).Reverse().ToList();
        var columnReplacements = columnsReferences
            .DistinctBy(columnReference => columnReference.Name.ToLowerInvariant())
            .Select(columnReference =>
            {
                var identifier = columnReference.Name;

                // Get the schema column corresponding to this identifier.
                var column = querySchemaColumns.FirstOrDefault(column =>
                    column is { BaseTableName.Length: > 0, BaseColumnName: { Length: > 0 } columnName } &&
                    string.Equals(columnName, identifier, StringComparison.InvariantCultureIgnoreCase));

                // Declare correct replacement of the identifier.
                var columnReplacement = column != null ? $"{column.TableName}.{column.BaseColumnName}" : null;

                return (Identifier: identifier, Replacement: columnReplacement);
            })
            .Where(columnReplacement => columnReplacement.Replacement != null)
            .ToDictionary(
                columnReplacement => columnReplacement.Identifier,
                columnReplacement => columnReplacement.Replacement,
                StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.IgnoreCase));
        columnsReferences = columnsReferences.Where(columnReference =>
            columnReplacements.TryGetValue(columnReference.Name, out _)).ToList();

        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        sqlCode = columnsReferences.Aggregate(sqlCode, (code, columnReference) =>
            _parser.ReplaceSqlParserObject(code, columnReference, columnReplacements[columnReference.Name]!));

        // Convert star expressions in explicit column declarations, to avoid loading columns of tables used for filtering.
        var starExpressions = _parser.GetWildcardExpressions(sqlCode).Reverse();
        var columnDeclarations = querySchemaColumns
            .Select(column => $"{column.TableName}.{column.BaseColumnName}");
        var startReplacement = string.Join(", ", columnDeclarations);

        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        sqlCode = starExpressions.Aggregate(sqlCode, (code, starExpression) =>
            _parser.ReplaceSqlParserObject(code, starExpression, startReplacement));

        return sqlCode;
    }

    public string ProcessSqlCodeAggregations(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        IList<QueryColumnAggregation> aggregations, out IDictionary<string, string[]> aggregationAliases)
    {
        // Get table columns original names.
        var columnDeclarations = aggregations.ToDictionary(aggregation => aggregation.QueryAlias,
            aggregation =>
            {
                var column = columns.FirstOrDefault(column =>
                    column.QueryAlias == aggregation.QueryAlias &&
                    column is { TableName.Length: > 0, BaseColumnName.Length: > 0 });

                if (column != null)
                {
                    var tableName = _parser.SqlAliasCleanup(column.TableName);
                    var columnName = _parser.SqlAliasCleanup(column.BaseColumnName);
                    return $"{tableName}.{columnName}";
                }

                var function = _parser.GetFunctionCall(sqlCode, aggregation.QueryAlias);
                return function is null ? aggregation.QueryAlias : function.Sql;
            });

        // Get aggregation aliases.
        var aliases = aggregations.ToDictionary(aggregation => aggregation.QueryAlias,
            aggregation => aggregation.Expressions.Select((_, i) => GetAggregationAlias(aggregation, i)).ToArray());

        // Build aggregation clauses in the format "<AggregationFunc>(<ColumnName>) AS <AggregationAlias>".
        var aggregationClauses = aggregations.SelectMany(aggregation =>
        {
            var columnName = columnDeclarations[aggregation.QueryAlias];
            return aggregation.Expressions.Select((_, i) =>
            {
                var expression = aggregation.Expressions[i];
                var aggregationAlias = aliases[aggregation.QueryAlias][i];

                return IsPredefinedAggregationFunction(expression)
                    ? $"{expression}({columnName}) AS {aggregationAlias}"
                    : $"{expression} AS {aggregationAlias}";
            });
        });

        // Replace SELECT statement for a new statement that consider only aggregation clauses.
        var selectClause = _parser.GetSelectClause(sqlCode);
        var replacement = $"SELECT {string.Join(", ", aggregationClauses)}";
        sqlCode = _parser.ReplaceSqlParserObject(sqlCode, selectClause, replacement);
        aggregationAliases = aliases;

        return sqlCode;

        // Return aggregation temporal alias.
        string GetAggregationAlias(QueryColumnAggregation aggregation, int i)
            => _parser.SqlAliasCleanup($"{aggregation.QueryAlias}_{i}");
    }

    public string ProcessSqlCodeUnions(string sqlCode)
    {
        var queryExpression = _parser.GetSelectStatement(sqlCode)?.QueryExpression;
        return queryExpression is not QueryBinaryExpression ? sqlCode : $"SELECT * FROM ({sqlCode}) AS UnionTable";
    }

    public async Task<IEnumerable<DbColumn>> ReadSqlQueryColumns(string sqlCode, CancellationToken ct = default)
    {
        // Try to use only the query specification instead of the whole sql code to read the schema.
        sqlCode = GetSqlCodeSelectStatement(sqlCode);

        // Read the column schema corresponding to this query from the database.
        const CommandBehavior behavior = CommandBehavior.SchemaOnly;
        await using var connection = new MySqlConnection(GetConnectionString());
        await using var command = new MySqlCommand(sqlCode, connection);
        await connection.OpenAsync(ct);
        await using var reader = await command.ExecuteReaderAsync(behavior, ct);

        return await reader.GetColumnSchemaAsync(ct);
    }

    public async Task<IEnumerable<object[]>> ReadSqlQueryData(string sqlCode, CancellationToken ct = default)
    {
        // Read the data corresponding to this query from the database.
        const CommandBehavior behavior = CommandBehavior.Default;
        await using var connection = new MySqlConnection(GetConnectionString());
        await using var command = new MySqlCommand(sqlCode, connection);
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

    private static bool IsPredefinedAggregationFunction(string expression) =>
        expression.Equals("avg", StringComparison.CurrentCultureIgnoreCase) ||
        expression.Equals("sum", StringComparison.CurrentCultureIgnoreCase) ||
        expression.Equals("min", StringComparison.CurrentCultureIgnoreCase) ||
        expression.Equals("max", StringComparison.CurrentCultureIgnoreCase);

    private IEnumerable<int> GetUserOrganizationIdsByFilterMode(QueryFilterMode? filterMode)
    {
        var userId = _loggedUserService.GetLoggedUserId()
                     ?? throw new BusinessException("Cannot get user Id");

        return filterMode switch
        {
            // Get the ids of the organisations to which this user belongs if filter mode is user-organizations filter.
            QueryFilterMode.UserOrganizationsFilter => _context.Set<UserOrganization>()
                .Where(userOrganization => userOrganization.UserId == userId)
                .Select(userOrganization => userOrganization.OrganizationId)
                .ToList(),
            // Get the id of this user organization if filter mode is user-organization filter.
            QueryFilterMode.UserOrganizationFilter when _context.Set<User>()
                .Where(user => user.Id == userId)
                .Select(user => user.OrganizationId)
                .FirstOrDefault() is { } organizationId => new[] { organizationId },
            _ => Enumerable.Empty<int>()
        };
    }

    private IEnumerable<TablesRelationExclusion> GetExcludedRelationsByFilterMode(QueryFilterMode? filterMode)
    {
        var usersTableName = _context.Model.FindEntityType(typeof(User))?.GetTableName()
                             ?? throw new BusinessException("Cannot find Users table.");

        return filterMode switch
        {
            // Exclude all relations from users table to any table different from
            // user-organizations table if filter mode is user-organizations filter.
            QueryFilterMode.UserOrganizationsFilter => new[]
            {
                new TablesRelationExclusion
                {
                    StartTableIdentifier = usersTableName,
                    EndTableIdentifier = _context.Model.FindEntityType(typeof(UserOrganization))?.GetTableName()
                                         ?? throw new BusinessException("Cannot find UserOrganizations table."),
                    ExclusionMode = ExclusionMode.ExcludeAllExceptEndTable
                }
            },
            // Exclude all relations from users table to any table different from
            // organizations table if filter mode is user-organization filter.
            QueryFilterMode.UserOrganizationFilter => new[]
            {
                new TablesRelationExclusion
                {
                    StartTableIdentifier = usersTableName,
                    EndTableIdentifier = _context.Model.FindEntityType(typeof(Organization))?.GetTableName()
                                         ?? throw new BusinessException("Cannot find Organizations table."),
                    ExclusionMode = ExclusionMode.ExcludeAllExceptEndTable
                }
            },
            _ => Enumerable.Empty<TablesRelationExclusion>()
        };
    }

    private string GetConnectionString()
        => _connectionString
           ?? throw new BusinessException("Connection string is not set for MySQL query provider.");
}