using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbQueryParser
{
    IEnumerable<string> GetUniqueTokens(string sqlCode, char prefix, string tokenType);

    SqlSelectStatement? GetSqlSelectStatement(string code);

    SqlSelectClause? GetSqlSelectClause(string code);

    SqlFromClause? GetSqlFromClause(string code);

    SqlOrderByClause? GetSqlOrderByClause(string code);

    SqlGroupByClause? GetSqlGroupByClause(string code);

    SqlWhereClause? GetSqlWhereClause(string code);

    SqlTableExpression? GetSqlTableExpression(string code, string name);

    SqlScalarFunctionCallExpression? GetSqlFunction(string code, string name);

    IEnumerable<SqlSelectStarExpression> GetSqlStarExpressions(string code);

    IEnumerable<SqlSelectScalarExpression> GetSqlDeclaredColumns(string code);

    IEnumerable<SqlColumnRefExpression> GetSqlColumnReferences(string code);

    IEnumerable<SqlTableRefExpression> GetSqlDeclaredTableAliases(string code);

    IEnumerable<SqlDerivedTableExpression> GetSqlDerivedTableExpressions(string code);

    IEnumerable<SqlCodeObject> GetSqlCodeObjects(string code, IEnumerable<string> patterns);

    string InsertBeforeSqlCodeObject(string code, SqlCodeObject? slqObject, string insertion);

    string InsertAfterSqlCodeObject(string code, SqlCodeObject? slqObject, string insertion);

    string ReplaceSqlCodeObject(string code, SqlCodeObject? slqObject, string replacement);

    string RemoveSqlCodeObject(string code, SqlCodeObject? slqObject);

    string SqlAliasCleanup(string alias);

    string SqlCodeCleanup(string code);

    string SqlCodeAliasesCleanup(string code);
}