using BBWM.Core.Data;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbQueryProcessorFactory
{
    void RegisterSqlQueryProvider<T>(DatabaseType type);
    IRqbQueryProcessor? GetSqlQueryProvider(DatabaseType type);
}