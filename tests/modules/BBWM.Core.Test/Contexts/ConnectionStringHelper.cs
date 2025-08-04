namespace BBWM.Core.Test.Contexts;

public static class ConnectionStringHelper
{
    /// <summary>
    /// Used for testing with InMemory data contexts with relations
    /// </summary>
    /// <returns></returns>
    public static string GetSqlLiteConnectionString() => "Filename=:memory:";

    /// <summary>
    /// Returns the connection string for MySql.
    /// Used in some tests without opening the connection to database.
    /// Used in database-specific tests with opened connection to database.
    ///
    /// Should be refactored to provide connection to real db server on the environments.
    /// </summary>
    /// <returns></returns>
    public static string GetMySqlConnectionString()
    {
        return $"server=test;port=9999;database=test-{Guid.NewGuid()};Uid=root;Pwd=P@ssword1";
        //return $"server=localhost;port=3306;database=test-{Guid.NewGuid()};Uid=root;Pwd=P@ssword1";
    }

    /// <summary>
    /// Returns the connection string for Sql Server.
    /// Used in some tests without opening the connection to database.
    /// Used in database-specific tests with opened connection to database.
    ///
    /// Should be refactored to provide connection to real db server on the environments.
    /// </summary>
    /// <returns></returns>
    public static string GetSqlServerConnectionString()
    {
        return $"Data Source=test;Initial Catalog=test-{Guid.NewGuid()};Integrated Security=true;";
        //return $"Data Source=.;Initial Catalog=test-{Guid.NewGuid()};Integrated Security=true;";
    }

    /// <summary>
    /// Used for testing with InMemory data contexts
    /// </summary>
    /// <returns></returns>
    public static string GetInMemoryConnectionString()
    {
        return Guid.NewGuid().ToString();
    }
}
