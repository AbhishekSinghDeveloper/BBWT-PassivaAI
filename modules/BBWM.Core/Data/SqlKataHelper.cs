using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace BBWM.Core.Data;

public static class SqlKataHelper
{
    public static QueryFactory GetQueryFactory(string connectionString, DatabaseType databaseType)
    {
        var builder = new DbContextOptionsBuilder<DbContext>();
        switch (databaseType)
        {
            case DatabaseType.MsSql:
                builder.UseSqlServer(connectionString);
                break;

            case DatabaseType.MySql:
                builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                break;

            default: throw new Exception($"DbConnection for database type {databaseType} not implemented.");
        }

        var dbConnection = new DbContext(builder.Options).Database.GetDbConnection();

        var compiler = dbConnection switch
        {
            MySqlConnection => new MySqlCompiler(),
            Microsoft.Data.SqlClient.SqlConnection => (Compiler)new SqlServerCompiler(),
            _ => throw new BusinessException($"DbConnection for database type {databaseType} not implemented.")
        };

        return new QueryFactory(dbConnection, compiler);
    }
}