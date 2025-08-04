using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.Model;
using BBF.Reporting.Core.Model.Variables;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBF.Reporting.QueryBuilder.Model;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Membership.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryProcessorDefault : IRqbQueryProcessorDefault
{
    private const char VarPrefix = '#';
    private const char ContextVarPrefix = '@';

    private readonly IDbContext _context;
    private readonly IRqbQueryParser _parser;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IContextVariableService _contextVariableService;
    private readonly IRqbQueryGraphService _tableGraphService;

    public RqbQueryProcessorDefault(
        IDbContext context,
        IRqbQueryParser parser,
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

    public string SqlAliasCleanup(string sqlAlias) => _parser.SqlAliasCleanup(sqlAlias);

    public string SqlCodeAliasesCleanup(string sqlCode) => _parser.SqlCodeAliasesCleanup(sqlCode);

    public string SqlCodeCleanup(string sqlCode) => _parser.SqlCodeCleanup(sqlCode);

    public SqlQueryValidateResult ValidateSqlCode(string sqlCode)
    {
        sqlCode = SqlCodeCleanup(sqlCode);

        if (sqlCode is not { Length: > 0 })
            return new SqlQueryValidateResult
            {
                Valid = false,
                Message = "SQL code cannot be empty."
            };

        return _parser.GetSqlSelectClause(ProcessSqlCodeUnions(sqlCode)) != null
            ? new SqlQueryValidateResult { Valid = true }
            : new SqlQueryValidateResult { Valid = false, Message = "Only SELECT queries are supported." };
    }

    public IEnumerable<(string TableName, string TableAlias)> GetSqlDeclaredTableAliases(string sqlCode)
        => _parser.GetSqlDeclaredTableAliases(sqlCode)
            .Where(table => table is { ObjectIdentifier.ObjectName.Value.Length: > 0, Alias.Value.Length: > 0 })
            .Select(table => (TableName: table.ObjectIdentifier.ObjectName.Value, TableAlias: table.Alias.Value));

    public IEnumerable<(string Expression, string TableAlias)> GetSqlDerivedTableExpressions(string sqlCode)
        => _parser.GetSqlDerivedTableExpressions(sqlCode)
            .Where(table => table is { QueryExpression.Sql.Length: > 0, Alias.Value.Length: > 0 })
            .Select(table => (Expression: table.QueryExpression.Sql, TableAlias: table.Alias.Value));

    public IEnumerable<string> GetQueryVariables(string sqlCode)
        => _parser.GetUniqueTokens(sqlCode, VarPrefix, RqbQueryParser.TokenIdTokenType);

    public string GetSqlCodeSelectStatement(string sqlCode)
    {
        var selectStatement = _parser.GetSqlSelectStatement(sqlCode);
        var querySpecification = selectStatement?.SelectSpecification?.QueryExpression as SqlQuerySpecification;
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

    public string ProcessSqlCodeSorting(string sqlCode, IEnumerable<QuerySchemaColumn> columns,
        string? sortingField, OrderDirection? sortingDirection)
    {
        var orderByClause = _parser.GetSqlOrderByClause(sqlCode);

        if (string.IsNullOrEmpty(sortingField))
        {
            if (orderByClause != null) return sqlCode;
            sortingField = columns.First().QueryAlias;
        }

        var direction = (sortingDirection ?? OrderDirection.Asc) == OrderDirection.Asc ? "ASC" : "DESC";
        var replacement = $"ORDER BY {sortingField} {direction}";
        return _parser.ReplaceSqlCodeObject(sqlCode, orderByClause, replacement);
    }

    public string ProcessSqlCodeVariables(string sqlCode, QueryVariables queryVariables)
    {
        // Get all declared variables.
        var declaredVariables = _parser.GetUniqueTokens(sqlCode, VarPrefix, RqbQueryParser.TokenIdTokenType).ToList();

        // Escape variables in the code to avoid errors.
        sqlCode = declaredVariables.Aggregate(sqlCode, (current, variableName) =>
        {
            var replacement = $"({VarPrefix}{variableName})";
            var pattern = $@"(?<!\w){VarPrefix}{variableName}(?!\w)";
            return Regex.Replace(current, pattern, replacement);
        });

        // Get instanced variables.
        var instancedVariables = queryVariables.Variables.Where(variable => !variable.Empty).ToList();

        // Replace all instanced variables by its corresponding value.
        sqlCode = instancedVariables.Aggregate(sqlCode, (current, variable) =>
        {
            var replacement = GetVariableValueToEmbed(variable);
            var pattern = $@"(?<!\w)\({VarPrefix}{variable.Name}\)(?!\w)";
            return Regex.Replace(current, pattern, replacement);
        });

        // Get null sql code objects that belong to boolean expressions.
        var nullValuePatterns = new[] { @"^\(\s*[^()]*(?<!\w)null(?!\w)[^()]*\)$", "^null$" };
        var nullSqlCodeObjects = _parser.GetSqlCodeObjects(sqlCode, nullValuePatterns)
            .Where(variableSqlCodeObject => variableSqlCodeObject.Parent is SqlBooleanExpression).Reverse().ToList();

        // Fix all boolean operations using null values.
        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        foreach (var nullSqlCodeObject in nullSqlCodeObjects)
        {
            switch (nullSqlCodeObject.Parent)
            {
                // If expression is an "IN" expression.
                case SqlInBooleanExpression inExpression:
                {
                    // Extract expression members.
                    var expression = inExpression.Sql;
                    var operand = inExpression.InExpression.Sql;
                    var @operator = inExpression.HasNot ? "IS NOT" : "IS";

                    // If some of them is missing, continue.
                    if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(operand)) continue;

                    // Otherwise replace it with a new expression with null check.
                    var replacement = $"({expression} OR {operand} {@operator} NULL)";
                    sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, inExpression, replacement);
                    break;
                }
                // If expression is a comparison expression.
                case SqlComparisonBooleanExpression comparisonExpression:
                {
                    // Extract expression members.
                    var expression = comparisonExpression.Sql;
                    var operand = comparisonExpression.Left == nullSqlCodeObject
                        ? comparisonExpression.Right.Sql
                        : comparisonExpression.Left.Sql;
                    var @operator = comparisonExpression.ComparisonOperator switch
                    {
                        SqlComparisonBooleanExpressionType.Equals => "IS",
                        SqlComparisonBooleanExpressionType.ValueEqual => "IS",
                        SqlComparisonBooleanExpressionType.LessThanOrEqual => "IS",
                        SqlComparisonBooleanExpressionType.GreaterThanOrEqual => "IS",
                        SqlComparisonBooleanExpressionType.NotEqual => "IS NOT",
                        _ => null
                    };

                    // If some of them is missing, continue.
                    if (string.IsNullOrEmpty(expression) || string.IsNullOrEmpty(operand) || string.IsNullOrEmpty(@operator)) continue;

                    // Otherwise replace it with a new expression with null check.
                    var replacement = $"({expression} OR {operand} {@operator} NULL)";
                    sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, comparisonExpression, replacement);
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
            var pattern = $@"(?<!\w)\({VarPrefix}{variable.Name}\)(?!\w)";
            return Regex.Replace(current, pattern, "(null)");
        });

        // Get remaining variable sql code objects that belong to boolean expressions.
        var variablePatterns = declaredVariables.Select(variable => $@"^\({VarPrefix}{variable}\)$").ToList();
        var variableSqlCodeObjects = _parser.GetSqlCodeObjects(sqlCode, variablePatterns)
            .Where(variableSqlCodeObject => variableSqlCodeObject.Parent is SqlBooleanExpression).Reverse();

        // Fix all remaining boolean expression using empty variables to ignore them.
        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        foreach (var variableSqlCodeObject in variableSqlCodeObjects)
        {
            var expression = variableSqlCodeObject.Parent.Sql;
            var replacement = $"({expression} OR 1 = 1)";
            sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, variableSqlCodeObject.Parent, replacement);
        }

        // Replace all unused variables by null, to force all operations using them to return unknown.
        sqlCode = declaredVariables.Aggregate(sqlCode, (current, variableName) =>
        {
            var pattern = $@"(?<!\w){VarPrefix}{variableName}(?!\w)";
            return Regex.Replace(current, pattern, "null");
        });

        return sqlCode;
    }

    // TODO: CRITICAL! We should take into account if query folder references external DB (not main DB) then
    // user context variables may not be applicable to the query because user context' DB has different data/record IDs.
    // The same problem is about auto-filtering by organizations - it cannot be applied to external DBs.
    public string ProcessSqlCodeContextVariables(string sqlCode)
    {
        var variables = _parser.GetUniqueTokens(sqlCode, ContextVarPrefix, RqbQueryParser.TokenVariableTokenType);

        return variables.Aggregate(sqlCode, (current, variableName) =>
            current.Replace(ContextVarPrefix + variableName, _contextVariableService.GetVariableValue(variableName) ?? "(null)"));
    }

    public string ProcessSqlCodeDataRowsCount(string sqlCode)
    {
        // Remove ORDER BY sentence to remove using of aliases outside the main query.
        var orderByClause = _parser.GetSqlOrderByClause(sqlCode);
        sqlCode = _parser.RemoveSqlCodeObject(sqlCode, orderByClause);

        var groupByClause = _parser.GetSqlGroupByClause(sqlCode);

        // If there is no group by clause, build a new query substituting the original SELECT clause.
        if (groupByClause == null)
        {
            var selectClause = _parser.GetSqlSelectClause(sqlCode)
                               ?? throw new BusinessException("Cannot find select clause in given sql query.");

            // If DISTINCT qualifier was applied, apply count over different combinations of the columns selected in the original query.
            // Otherwise, just count all records.
            var replacement = selectClause.IsDistinct
                ? $"SELECT COUNT(DISTINCT {string.Join(", ", selectClause.SelectExpressions.Select(expression =>
                    expression is SqlSelectScalarExpression scalar ? scalar.Expression.Sql : expression.Sql))}) AS _count"
                : "SELECT COUNT(1) AS _count";

            // Replace the old SELECT clause with the new one.
            sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, selectClause, replacement);
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
        var destinyTableAlias = _parser.GetSqlTableExpression(sqlCode, destinyTableName) switch
        {
            // If it's an aliased table reference declaration, substitute its base name with the alias, for the join clause.
            SqlTableRefExpression { Alias: not null } tableReference => tableReference.Alias.Value,
            // If it's into a derived table declaration, substitute its base name with the alias, for the join clause.
            SqlDerivedTableExpression { Alias: not null } derivedTable => derivedTable.Alias.Value,
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
            var tableDeclaration = _parser.GetSqlTableExpression(sqlCode, source.ParentTableName);

            source.ParentTableName = tableDeclaration switch
            {
                // If table declaration it's not found, throw error.
                null => throw new BusinessException("Initial table is not declared correctly."),
                // If it's a aliased table reference declaration, substitute its base name with the alias, for the join clause.
                SqlTableRefExpression { Alias: not null } tableReference => tableReference.Alias.Value,
                // If it's into a derived table declaration, substitute its base name with the alias, for the join clause.
                SqlDerivedTableExpression { Alias: not null } derivedTable => derivedTable.Alias.Value,
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
            sqlCode = _parser.InsertAfterSqlCodeObject(sqlCode, tableDeclaration, joinInsertion);
        }

        // Get organization ids corresponding to this user.
        var organizationsIds = GetUserOrganizationIdsByFilterMode(filterMode).ToList();

        var whereClause = _parser.GetSqlWhereClause(sqlCode);
        // If this user has no organisations, block the user's access to all records.
        if (organizationsIds.Count == 0)
        {
            const string blockingClause = " WHERE 0 = 1";
            // Place the blocking clause after the FROM clause if there is no WHERE clause in the query.
            // Set the blocking clause as a replacement of the WHERE clause otherwise.
            if (whereClause == null)
            {
                var fromClause = _parser.GetSqlFromClause(sqlCode);
                sqlCode = _parser.InsertAfterSqlCodeObject(sqlCode, fromClause, blockingClause);
            }
            else _parser.ReplaceSqlCodeObject(sqlCode, whereClause, blockingClause);
        }
        // Otherwise, build the new filtering clause and introduce it in the query.
        // Build the new filtering clause as a WHERE clause if there is no WHERE clause in the query.
        else if (whereClause == null)
        {
            var fromClause = _parser.GetSqlFromClause(sqlCode);
            var filter = $"({string.Join(", ", organizationsIds)})";
            var whereInsertion = $" WHERE {destinyTableAlias}.{destinyPrimaryKey} IN {filter}";
            sqlCode = _parser.InsertAfterSqlCodeObject(sqlCode, fromClause, whereInsertion);
        }
        // Otherwise, set it as appendix of the existing WHERE clause.
        else
        {
            var whereExpression = whereClause.Expression.Sql;
            var filter = $"({string.Join(", ", organizationsIds)})";
            var whereReplacement = $"WHERE ({whereExpression}) AND {destinyTableAlias}.{destinyPrimaryKey} IN {filter}";
            sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, whereClause, whereReplacement);
        }

        // Make column declaration explicit in select clause, to avoid ambiguity with tables used for filtering.
        var columnsReferences = _parser.GetSqlColumnReferences(sqlCode).Reverse().ToList();
        var columnReplacements = columnsReferences
            .DistinctBy(columnReference => columnReference.ColumnName.Value.ToLowerInvariant())
            .Select(columnReference =>
            {
                var identifier = columnReference.ColumnName.Value;

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
            columnReplacements.TryGetValue(columnReference.ColumnName.Value, out _)).ToList();

        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        sqlCode = columnsReferences.Aggregate(sqlCode, (code, columnReference) =>
            _parser.ReplaceSqlCodeObject(code, columnReference, columnReplacements[columnReference.ColumnName.Value]!));

        // Convert star expressions in explicit column declarations, to avoid loading columns of tables used for filtering.
        var starExpressions = _parser.GetSqlStarExpressions(sqlCode)
            .Where(expression => expression is { Sql: "*" }).Reverse();
        var columnDeclarations = querySchemaColumns
            .Select(column => $"{column.TableName}.{column.BaseColumnName}");
        var startReplacement = string.Join(", ", columnDeclarations);

        // Visit the column expressions in reverse order so as not to affect the relative position
        // of the remaining tokens to be visited after a replacement.
        sqlCode = starExpressions.Aggregate(sqlCode, (code, starExpression) =>
            _parser.ReplaceSqlCodeObject(code, starExpression, startReplacement));

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

                var function = _parser.GetSqlFunction(sqlCode, aggregation.QueryAlias);
                return function?.Arguments is not { Count: > 0 } ? aggregation.QueryAlias : function.Sql;
            });

        // Return aggregation temporal alias.
        string GetAggregationAlias(QueryColumnAggregation aggregation, int i)
            => _parser.SqlAliasCleanup($"{aggregation.QueryAlias}_{i}");

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
        var selectClause = _parser.GetSqlSelectClause(sqlCode);
        var replacement = $"SELECT {string.Join(", ", aggregationClauses)}";
        sqlCode = _parser.ReplaceSqlCodeObject(sqlCode, selectClause, replacement);
        aggregationAliases = aliases;

        return sqlCode;
    }

    public string ProcessSqlCodeUnions(string sqlCode)
    {
        var queryExpression = _parser.GetSqlSelectStatement(sqlCode)?.SelectSpecification?.QueryExpression;
        return queryExpression is not SqlBinaryQueryExpression ? sqlCode : $"SELECT * FROM ({sqlCode}) AS UnionTable";
    }

    //TODO: For now we don't take into account variable type
    private static string GetVariableValueToEmbed(EmittedVariable variable)
    {
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

        // Method to convert object value in a list of string values, if possible.
        string GetStringValueList(object? rawValue)
        {
            if (rawValue is not IEnumerable enumerable) return "(null)";
            var values = enumerable.Cast<object>().Select(GetStringValue).ToList();
            return values.Count > 0 ? $"({string.Join(", ", values)})" : "(null)";
        }

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
}