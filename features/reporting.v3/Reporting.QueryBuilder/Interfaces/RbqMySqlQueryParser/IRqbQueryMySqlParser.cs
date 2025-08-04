using BBF.Reporting.QueryBuilder.Model.ParserModels;

namespace BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;

public interface IRqbQueryMySqlParser
{
    SelectStatement? GetSelectStatement(string code);

    SelectClause? GetSelectClause(string code);

    FromClause? GetFromClause(string code);

    OrderByClause? GetOrderByClause(string code);

    LimitClause? GetLimitClause(string code);

    GroupByClause? GetGroupByClause(string code);

    WhereClause? GetWhereClause(string code);

    FunctionCall? GetFunctionCall(string code, string name);

    TableExpression? GetTableExpression(string code, string name);

    IEnumerable<WildcardExpression> GetWildcardExpressions(string code);

    IEnumerable<SelectExpression> GetSelectExpressions(string code);

    IEnumerable<TableReference> GetTableReferences(string code);

    IEnumerable<DerivedTableExpression> GetDerivedTableExpressions(string code);

    IEnumerable<TParserObject> GetSqlParserObjectsOfType<TParserObject>(SqlParserObject? sqlParserObject, IList<Type>? walkInto = null)
        where TParserObject : SqlParserObject;

    IEnumerable<TParserObject> GetSqlParserObjectsOfType<TParserObject>(string code, IList<Type>? walkInto = null)
        where TParserObject : SqlParserObject;

    string InsertBeforeSqlParserObject(string code, SqlParserObject? sqlParserObject, string insertion);

    string InsertAfterSqlParserObject(string code, SqlParserObject? sqlParserObject, string insertion);

    string ReplaceSqlParserObject(string code, SqlParserObject? sqlParserObject, string replacement);

    string RemoveSqlParserObject(string code, SqlParserObject? sqlParserObject);

    string SqlAliasCleanup(string alias);

    string SqlCodeCleanup(string code);

    string SqlCodeAliasesCleanup(string code);
}