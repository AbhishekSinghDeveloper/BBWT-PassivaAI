using System.Text.RegularExpressions;
using BBF.Reporting.QueryBuilder.Interfaces;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbQueryParser : IRqbQueryParser
{
    public const string TokenIdTokenType = "TOKEN_ID";
    public const string TokenVariableTokenType = "TOKEN_VARIABLE";


    public IEnumerable<string> GetUniqueTokens(string sqlCode, char prefix, string tokenType)
    {
        var result = Parser.Parse(sqlCode);
        var varTokens = result.Script.Tokens
            .Where(token => token.Text.StartsWith(prefix) && token.Type == tokenType)
            .Select(token => token.Text.TrimStart(prefix)).Distinct();
        return varTokens;
    }

    public SqlSelectStatement? GetSqlSelectStatement(string code)
    {
        if (string.IsNullOrEmpty(code)) return null;

        // MySQL parser test
        // TODO: investigate if SqlParserCS can be used for MySQL parser with the same
        // full functional as current MS SQL parser.
        // OR! We can try using SqlParserCS for all DB types?
        //var mysqlResult = new SqlParser.Parser().ParseSql(code);

        // Parse the SQL query
        var result = Parser.Parse(code);

        // Locate the root-level SELECT statement
        var sqlBatch = result.Script.Batches is { Count: > 0 }
            ? result.Script.Batches[0]
            : null;

        return sqlBatch?.Statements is { Count: > 0 }
            ? sqlBatch.Statements[0] as SqlSelectStatement
            : null;
    }

    public SqlSelectClause? GetSqlSelectClause(string code)
    {
        // Locate the root-level SELECT statement
        var selectStatement = GetSqlSelectStatement(code);
        // Get SELECT clause.
        var queryExpression = selectStatement?.SelectSpecification?.QueryExpression;

        switch (queryExpression)
        {
            case SqlQuerySpecification selectSpecification:
            {
                var selectClause = selectSpecification.SelectClause;
                return selectClause;
            }
            case SqlBinaryQueryExpression unionExpression:
            {
                var selectClause = GetMainSelectClause(unionExpression);
                return selectClause;

                SqlSelectClause? GetMainSelectClause(SqlBinaryQueryExpression expression)
                    => expression.Left switch
                    {
                        SqlQuerySpecification querySpecification => querySpecification.SelectClause,
                        SqlBinaryQueryExpression binaryQueryExpression => GetMainSelectClause(binaryQueryExpression),
                        _ => null
                    };
            }
            default:
                return null;
        }
    }

    public SqlFromClause? GetSqlFromClause(string code)
    {
        // Locate the root-level SELECT statement
        var selectStatement = GetSqlSelectStatement(code);
        // Get FROM clause.
        var selectSpecification = selectStatement?.SelectSpecification?.QueryExpression as SqlQuerySpecification;
        var selectClause = selectSpecification?.FromClause;

        return selectClause;
    }

    public SqlOrderByClause? GetSqlOrderByClause(string code)
    {
        // Locate the root-level SELECT statement
        var selectStatement = GetSqlSelectStatement(code);
        // Get ORDER BY clause.
        var selectSpecification = selectStatement?.SelectSpecification;
        var orderByClause = selectSpecification?.OrderByClause;

        return orderByClause;
    }

    public SqlGroupByClause? GetSqlGroupByClause(string code)
    {
        // Locate the root-level SELECT statement
        var selectStatement = GetSqlSelectStatement(code);
        // Get GROUP BY clause.
        var selectSpecification = selectStatement?.SelectSpecification?.QueryExpression as SqlQuerySpecification;
        var groupByClause = selectSpecification?.GroupByClause;

        return groupByClause;
    }

    public SqlWhereClause? GetSqlWhereClause(string code)
    {
        // Locate the root-level SELECT statement
        var selectStatement = GetSqlSelectStatement(code);
        // Get WHERE clause.
        var selectSpecification = selectStatement?.SelectSpecification?.QueryExpression as SqlQuerySpecification;
        var whereClause = selectSpecification?.WhereClause;

        return whereClause;
    }

    public SqlScalarFunctionCallExpression? GetSqlFunction(string code, string name)
    {
        // Get SELECT clause.
        var selectClause = GetSqlSelectClause(code);
        if (selectClause?.SelectExpressions is not { Count: > 0 }) return null;

        // Find the function that has this name.
        var function = selectClause.SelectExpressions
            .Select(expression => expression as SqlSelectScalarExpression)
            .FirstOrDefault(expression =>
                expression is { Expression: SqlScalarFunctionCallExpression } &&
                string.Equals(expression.Alias?.Value, name, StringComparison.InvariantCultureIgnoreCase))
            ?.Expression as SqlScalarFunctionCallExpression;

        return function;
    }

    private SqlTableExpression? GetSqlTableExpression(IEnumerable<SqlTableExpression> expressions, string name)
    {
        var declarations =
            from expression in expressions
            select expression switch
            {
                // If there is a join expression, resolve its members recursively.
                SqlJoinTableExpression @join => GetSqlTableExpression(new[] { @join.Left, @join.Right }, name),
                // If the table belongs to a derived table declaration, return the derived table declaration.
                SqlDerivedTableExpression derived when
                    GetSqlTableExpression(derived.QueryExpression.Sql, name) != null => derived,
                // If the table is a table reference declaration, return the table reference declaration.
                SqlTableRefExpression @ref when
                    string.Equals(@ref.ObjectIdentifier?.ObjectName?.Value,
                        name, StringComparison.InvariantCultureIgnoreCase) => @ref,
                // Ignore remaining expressions.
                _ => null
            };

        // Find first non null declaration.
        return declarations.FirstOrDefault(declaration => declaration != null);
    }

    public SqlTableExpression? GetSqlTableExpression(string code, string name)
    {
        // Locate the root-level FROM clause.
        var fromClause = GetSqlFromClause(code);

        // Find this table declaration between table expressions.
        return fromClause?.TableExpressions is { Count: > 0 }
            ? GetSqlTableExpression(fromClause.TableExpressions, name)
            : null;
    }

    public IEnumerable<SqlSelectStarExpression> GetSqlStarExpressions(string code)
    {
        // Locate the root-level SELECT clause.
        var selectClause = GetSqlSelectClause(code);

        if (selectClause?.SelectExpressions is not { Count: > 0 })
            return new List<SqlSelectStarExpression>();

        // Return only select star expressions (global or table specific).
        var declarations =
            from expression in selectClause.SelectExpressions
            let declaration = expression as SqlSelectStarExpression
            where declaration != null
            select declaration;

        return declarations;
    }

    public IEnumerable<SqlSelectScalarExpression> GetSqlDeclaredColumns(string code)
    {
        // Locate the root-level SELECT clause.
        var selectClause = GetSqlSelectClause(code);

        if (selectClause?.SelectExpressions is not { Count: > 0 })
            return new List<SqlSelectScalarExpression>();

        // Return only select scalar expressions (column declarations or operation column declarations).
        var declarations =
            from expression in selectClause.SelectExpressions
            let declaration = expression as SqlSelectScalarExpression
            where declaration != null
            select declaration;

        return declarations;
    }

    private static IEnumerable<SqlColumnRefExpression> GetSqlColumnReferences(IEnumerable<SqlCodeObject> expressions)
    {
        var declarations =
            from expression in expressions
            from declaration in expression switch
            {
                SqlColumnRefExpression @ref => new[] { @ref },
                _ when expression.Children.Any() => GetSqlColumnReferences(expression.Children),
                _ => Array.Empty<SqlColumnRefExpression>()
            }
            select declaration;

        return declarations;
    }

    public IEnumerable<SqlColumnRefExpression> GetSqlColumnReferences(string code)
    {
        var selectStatement = GetSqlSelectStatement(code);

        return selectStatement != null && selectStatement.Children.Any()
            ? GetSqlColumnReferences(selectStatement.Children)
            : new List<SqlColumnRefExpression>();
    }

    private static IEnumerable<SqlTableRefExpression> GetSqlDeclaredTableAliases(IEnumerable<SqlTableExpression> expressions)
    {
        var declarations =
            from expression in expressions
            from declaration in expression switch
            {
                // If there is a join expression, resolve its members recursively.
                SqlJoinTableExpression @join => GetSqlDeclaredTableAliases(new[] { @join.Left, @join.Right }),
                // Return only table reference expressions with declared table name and alias.
                SqlTableRefExpression { ObjectIdentifier.ObjectName: not null, Alias: not null } @ref => new[] { @ref },
                // Ignore remaining expressions.
                _ => Array.Empty<SqlTableRefExpression>()
            }
            select declaration;

        return declarations;
    }

    public IEnumerable<SqlTableRefExpression> GetSqlDeclaredTableAliases(string code)
    {
        // Locate the root-level FROM clause.
        var fromClause = GetSqlFromClause(code);

        // Return only table reference expressions (table declarations with table name and aliases).
        return fromClause?.TableExpressions is { Count: > 0 }
            ? GetSqlDeclaredTableAliases(fromClause.TableExpressions)
            : new List<SqlTableRefExpression>();
    }

    private static IEnumerable<SqlDerivedTableExpression> GetSqlDerivedTableExpressions(IEnumerable<SqlTableExpression> expressions)
    {
        var declarations =
            from expression in expressions
            from declaration in expression switch
            {
                // If there is a join expression, resolve its members recursively.
                SqlJoinTableExpression @join => GetSqlDerivedTableExpressions(new[] { @join.Left, @join.Right }),
                // Return only derived table expressions with declared sql query and alias.
                SqlDerivedTableExpression { QueryExpression.Sql: not null, Alias: not null } @ref => new[] { @ref },
                // Ignore remaining expressions.
                _ => Array.Empty<SqlDerivedTableExpression>()
            }
            select declaration;

        return declarations;
    }

    public IEnumerable<SqlDerivedTableExpression> GetSqlDerivedTableExpressions(string code)
    {
        // Locate the root-level FROM clause.
        var fromClause = GetSqlFromClause(code);

        // Return only derived table expressions (table declaration as table expressions, with aliases).
        return fromClause?.TableExpressions is { Count: > 0 }
            ? GetSqlDerivedTableExpressions(fromClause.TableExpressions)
            : new List<SqlDerivedTableExpression>();
    }

    private static IEnumerable<SqlCodeObject> GetSqlCodeObjects(IEnumerable<SqlCodeObject> expressions, IList<string> patterns)
    {
        var declarations =
            from expression in expressions
            from declaration in expression switch
            {
                _ when patterns.Any(pattern => Regex.IsMatch(expression.Sql, pattern)) => new[] { expression },
                _ when expression.Children.Any() => GetSqlCodeObjects(expression.Children, patterns),
                _ => Array.Empty<SqlCodeObject>()
            }
            select declaration;

        return declarations;
    }

    public IEnumerable<SqlCodeObject> GetSqlCodeObjects(string code, IEnumerable<string> patterns)
    {
        var selectStatement = GetSqlSelectStatement(code);

        return selectStatement != null && selectStatement.Children.Any() &&
               patterns.ToList() is { Count: > 0 } contents
            ? GetSqlCodeObjects(selectStatement.Children, contents)
            : new List<SqlCodeObject>();
    }

    public string InsertBeforeSqlCodeObject(string code, SqlCodeObject? slqObject, string insertion)
    {
        return slqObject == null
            ? string.Concat(code, " ", insertion)
            : code.Insert(slqObject.StartLocation.Offset, insertion);
    }

    public string InsertAfterSqlCodeObject(string code, SqlCodeObject? slqObject, string insertion)
    {
        return slqObject == null
            ? string.Concat(code, " ", insertion)
            : code.Insert(slqObject.EndLocation.Offset, insertion);
    }

    public string ReplaceSqlCodeObject(string code, SqlCodeObject? slqObject, string replacement)
    {
        if (slqObject == null) return string.Concat(code, " ", replacement);

        var queryPrefix = code[..slqObject.StartLocation.Offset];
        var querySuffix = code[slqObject.EndLocation.Offset..];

        return string.Concat(
            queryPrefix,
            queryPrefix.Length > 0 ? " " : null,
            replacement,
            querySuffix.Length < code.Length ? " " : null,
            querySuffix);
    }

    public string RemoveSqlCodeObject(string code, SqlCodeObject? slqObject)
    {
        if (slqObject == null) return code;

        var queryPrefix = code[..slqObject.StartLocation.Offset];
        var querySuffix = code[slqObject.EndLocation.Offset..];

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
        var declarations = GetSqlDeclaredColumns(code).ToList();

        if (declarations is not { Count: > 0 }) return code;

        foreach (var declaration in declarations)
        {
            if (declaration.Alias != null)
            {
                var originalAlias = declaration.Alias.Sql;
                var validAlias = SqlAliasCleanup(declaration.Alias.Value);

                // If the alias is valid, do nothing.
                if (validAlias == originalAlias || originalAlias == null) continue;

                // Otherwise, substitute it in all the string.
                var pattern = $@"(?<!\w){Regex.Escape(originalAlias)}(?<!\w)";
                code = Regex.Replace(code, pattern, validAlias);
            }
            else if (declaration.Expression != null)
            {
                var expression = declaration.Expression.Sql;
                var validAlias = SqlAliasCleanup(declaration.Expression.Sql);

                // If there is no expression, do nothing.
                if (expression == null) continue;

                // Add a valid alias to this column declaration.
                var pattern = $@"(?<!\w){Regex.Escape(expression)}(?<!\w)";
                code = Regex.Replace(code, pattern, $"{expression} AS {validAlias}");
            }
        }

        return code;
    }
}