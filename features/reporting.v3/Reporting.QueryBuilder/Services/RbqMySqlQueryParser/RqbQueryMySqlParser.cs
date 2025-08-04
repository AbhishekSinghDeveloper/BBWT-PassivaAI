using System.Text.RegularExpressions;
using BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;
using BBF.Reporting.QueryBuilder.Model.ParserModels;

namespace BBF.Reporting.QueryBuilder.Services.RbqMySqlQueryParser;

public class RqbQueryMySqlParser : IRqbQueryMySqlParser
{
    private readonly IRqbQueryMySqlParserHelper _helper;

    public RqbQueryMySqlParser(IRqbQueryMySqlParserHelper helper)
        => _helper = helper;

    public SelectStatement? GetSelectStatement(string code)
    {
        if (string.IsNullOrEmpty(code)) return null;

        // Parse the SQL query.
        var statements = _helper.Parse(code);

        return statements is { Count: > 0 }
            ? statements[0] as SelectStatement
            : null;
    }

    public SelectClause? GetSelectClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get SELECT clause.
        var queryExpression = selectStatement?.QueryExpression;

        switch (queryExpression)
        {
            case QuerySpecification selectSpecification:
            {
                var selectClause = selectSpecification.SelectClause;
                return selectClause;
            }
            case QueryBinaryExpression binaryExpression:
            {
                var selectClause = GetMainSelectClause(binaryExpression);
                return selectClause;

                SelectClause? GetMainSelectClause(QueryBinaryExpression expression)
                    => expression.Left switch
                    {
                        QuerySpecification querySpecification => querySpecification.SelectClause,
                        QueryBinaryExpression queryBinaryExpression => GetMainSelectClause(queryBinaryExpression),
                        _ => null
                    };
            }
            default:
                return null;
        }
    }

    public FromClause? GetFromClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get FROM clause.
        var selectSpecification = selectStatement?.QueryExpression as QuerySpecification;
        var selectClause = selectSpecification?.FromClause;

        return selectClause;
    }

    public OrderByClause? GetOrderByClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get ORDER BY clause.
        var orderByClause = selectStatement?.OrderByClause;

        return orderByClause;
    }

    public LimitClause? GetLimitClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get ORDER BY clause.
        var limitClause = selectStatement?.LimitClause;

        return limitClause;
    }

    public GroupByClause? GetGroupByClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get GROUP BY clause.
        var selectSpecification = selectStatement?.QueryExpression as QuerySpecification;
        var groupByClause = selectSpecification?.GroupByClause;

        return groupByClause;
    }

    public WhereClause? GetWhereClause(string code)
    {
        // Locate the root-level SELECT statement.
        var selectStatement = GetSelectStatement(code);
        // Get WHERE clause.
        var selectSpecification = selectStatement?.QueryExpression as QuerySpecification;
        var whereClause = selectSpecification?.WhereClause;

        return whereClause;
    }

    public FunctionCall? GetFunctionCall(string code, string alias)
    {
        // Get the SELECT clause.
        var selectClause = GetSelectClause(code);
        if (selectClause?.SelectExpressions.ToList() is not { Count: > 0 } selectionList) return null;

        // Find the function column declaration that has this alias.
        var function = selectionList.FirstOrDefault(selection =>
            selection is { FunctionCall: not null } &&
            string.Equals(selection.Alias, alias, StringComparison.InvariantCultureIgnoreCase))?.FunctionCall;

        return function;
    }

    public TableExpression? GetTableExpression(string code, string alias)
    {
        // Get the FROM clause.
        var fromClause = GetFromClause(code);
        var tableExpressions = GetSqlParserObjectsOfType<TableExpression>(fromClause);

        // Find the table expression that has this alias.
        var tableExpression = tableExpressions.FirstOrDefault(expression =>
            string.Equals(expression.Alias, alias, StringComparison.InvariantCultureIgnoreCase));

        return tableExpression;
    }

    public IEnumerable<WildcardExpression> GetWildcardExpressions(string code)
    {
        // Locate the root-level SELECT clause.
        var selectClause = GetSelectClause(code);

        return selectClause?.Wildcards is not null
            ? selectClause.Wildcards
            : new List<WildcardExpression>();
    }

    public IEnumerable<SelectExpression> GetSelectExpressions(string code)
    {
        // Locate the root-level SELECT clause.
        var selectClause = GetSelectClause(code);

        return selectClause?.SelectExpressions is not null
            ? selectClause.SelectExpressions
            : new List<SelectExpression>();
    }

    public IEnumerable<TableReference> GetTableReferences(string code)
    {
        // Get the FROM clause.
        var fromClause = GetFromClause(code);

        // Only walk into table join expressions.
        var walkInto = new List<Type> { typeof(TableJoinExpression) };

        return GetSqlParserObjectsOfType<TableReference>(fromClause, walkInto);
    }

    public IEnumerable<DerivedTableExpression> GetDerivedTableExpressions(string code)
    {
        // Get the FROM clause.
        var fromClause = GetFromClause(code);

        // Only walk into table join expressions.
        var walkInto = new List<Type> { typeof(TableJoinExpression) };

        return GetSqlParserObjectsOfType<DerivedTableExpression>(fromClause, walkInto);
    }

    private static IEnumerable<TParserObject> GetSqlParserObjectsOfType<TParserObject>(IList<SqlParserObject> expressions, IList<Type>? walkInto = null)
        where TParserObject : SqlParserObject
    {
        foreach (var expression in expressions)
        {
            switch (expression)
            {
                case TParserObject sqlParserObject:
                {
                    yield return sqlParserObject;
                    continue;
                }

                case not null when walkInto != null && !walkInto.Any(type => type.IsInstanceOfType(expression)):
                    continue;

                case not null when expression.Children.Any():
                {
                    var children = GetSqlParserObjectsOfType<TParserObject>(expression.Children);
                    foreach (var child in children)
                        yield return child;
                    break;
                }
            }
        }
    }

    public IEnumerable<TParserObject> GetSqlParserObjectsOfType<TParserObject>(SqlParserObject? sqlParserObject, IList<Type>? walkInto = null)
        where TParserObject : SqlParserObject
    {
        return sqlParserObject?.Children is { Count: > 0 }
            ? GetSqlParserObjectsOfType<TParserObject>(sqlParserObject.Children, walkInto)
            : new List<TParserObject>();
    }

    public IEnumerable<TParserObject> GetSqlParserObjectsOfType<TParserObject>(string code, IList<Type>? walkInto = null)
        where TParserObject : SqlParserObject
        => GetSqlParserObjectsOfType<TParserObject>(GetSelectStatement(code));

    public string InsertBeforeSqlParserObject(string code, SqlParserObject? sqlParserObject, string insertion)
    {
        return sqlParserObject == null
            ? string.Concat(code, " ", insertion)
            : code.Insert(sqlParserObject.Range.Start.Value, insertion + " ");
    }

    public string InsertAfterSqlParserObject(string code, SqlParserObject? sqlParserObject, string insertion)
    {
        return sqlParserObject == null
            ? string.Concat(code, " ", insertion)
            : code.Insert(sqlParserObject.Range.End.Value, " " + insertion);
    }

    public string ReplaceSqlParserObject(string code, SqlParserObject? sqlParserObject, string replacement)
    {
        if (sqlParserObject == null) return string.Concat(code, " ", replacement);

        var queryPrefix = code[..sqlParserObject.Range.Start];
        var querySuffix = code[sqlParserObject.Range.End..];

        return string.Concat(
            queryPrefix,
            queryPrefix.Length > 0 ? " " : null,
            replacement,
            querySuffix.Length < code.Length ? " " : null,
            querySuffix);
    }

    public string RemoveSqlParserObject(string code, SqlParserObject? sqlParserObject)
    {
        if (sqlParserObject == null) return code;

        var queryPrefix = code[..sqlParserObject.Range.Start];
        var querySuffix = code[sqlParserObject.Range.End..];

        return string.Concat(queryPrefix, querySuffix);
    }

    public string SqlCodeCleanup(string code)
    {
        //This will remove "any kind of whitespace or invisible separator".
        code = Regex.Replace(code, @"\p{Z}", " ");

        return Regex.Replace(code, @"\t|\n|\r", " ").Trim();
    }

    public string SqlAliasCleanup(string alias)
    {
        return !string.IsNullOrEmpty(alias)
            ? Regex.Replace(alias, @"[^a-zA-Z0-9_$]", "_")
            : string.Empty;
    }

    public string SqlCodeAliasesCleanup(string code)
    {
        code = SqlCodeCleanup(code);

        // Get the SELECT column declarations.
        var declarations = GetSelectExpressions(code).ToList();

        if (declarations is not { Count: > 0 }) return code;

        foreach (var declaration in declarations)
        {
            if (declaration.Alias != null)
            {
                var originalAlias = declaration.Alias;
                var validAlias = SqlAliasCleanup(declaration.Alias);

                // If the alias is valid, do nothing.
                if (validAlias == originalAlias || originalAlias == null) continue;

                // Otherwise, substitute it in all the string.
                var pattern = $@"(?<!\w){Regex.Escape(originalAlias)}(?<!\w)";
                code = Regex.Replace(code, pattern, validAlias);
            }
            else
            {
                var expression = declaration.Sql;
                var validAlias = SqlAliasCleanup(declaration.Sql);

                // Add a valid alias to this column declaration.
                var pattern = $@"(?<!\w){Regex.Escape(expression)}(?<!\w)";
                code = Regex.Replace(code, pattern, $"{expression} AS {validAlias}");
            }
        }

        return code;
    }
}