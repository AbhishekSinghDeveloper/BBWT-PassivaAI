using BBF.Reporting.QueryBuilder.Model.ParserModels;

namespace BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;

public interface IRqbQueryMySqlParserHelper
{
    List<SqlParserObject> Parse(string code);
}