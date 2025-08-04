using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace BBWM.Core.Data;

public static class DbContextExtensions
{
    /// <summary>
    /// Gets database type from database facade
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static DatabaseType GetDatabaseType(this IDbContext dbContext)
    {
        if (dbContext.Database.IsMySql())
            return DatabaseType.MySql;
        if (dbContext.Database.IsSqlServer())
            return DatabaseType.MsSql;
        if (dbContext.Database.IsNpgsql())
            return DatabaseType.PostgreSql;
        throw new NotSupportedException("Database type of DB context not identified.");
    }

    /// <summary>
    /// Gets database type from connection string
    /// </summary>
    public static DatabaseType GetDatabaseType(this string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new BusinessException("The connection string cannot be null or empty.");

        const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

        if (connectionString.Contains("Server=", comparison) &&
            connectionString.Contains("Database=", comparison) &&
            (connectionString.Contains("Uid=", comparison) ||
             connectionString.Contains("User ID=", comparison)))
            return DatabaseType.MySql;

        if (connectionString.Contains("Server=", comparison) &&
            connectionString.Contains("Database=", comparison) ||
            connectionString.Contains("Data Source=", comparison) &&
            connectionString.Contains("Initial Catalog=", comparison))
            return DatabaseType.MsSql;

        throw new BusinessException("Unsupported database type");
    }

    public static IServiceCollection AddDbContext<TService, TImplementation>(
        this IServiceCollection services,
        DbContextOptions<TImplementation> options,
        Func<IServiceProvider, IEnumerable<object>> extraParamsFactory = null)
        where TService : class
        where TImplementation : DbContext, TService
    {
        services.AddScoped<TService, TImplementation>(serviceProvider =>
        {
            var args = new List<object>() { options };

            if (extraParamsFactory is not null)
            {
                args.AddRange(extraParamsFactory(serviceProvider));
            }

            return (TImplementation)Activator.CreateInstance(typeof(TImplementation), args.ToArray());
        });

        return services;
    }

    public static MySqlDbContextOptionsBuilder EnableRetryOnFailure(this MySqlDbContextOptionsBuilder builder, IDatabaseConnectionSettings connectionSettings) =>
        builder.EnableRetryOnFailure(
            connectionSettings.MaxRetryCount,
            TimeSpan.FromSeconds(connectionSettings.MaxRetryDelay),
            connectionSettings.ErrorNumbersToAdd);

    public static SqlServerDbContextOptionsBuilder EnableRetryOnFailure(this SqlServerDbContextOptionsBuilder builder, IDatabaseConnectionSettings connectionSettings) =>
        builder.EnableRetryOnFailure(
            connectionSettings.MaxRetryCount,
            TimeSpan.FromSeconds(connectionSettings.MaxRetryDelay),
            connectionSettings.ErrorNumbersToAdd);

    public static NpgsqlDbContextOptionsBuilder EnableRetryOnFailure(this NpgsqlDbContextOptionsBuilder builder, IDatabaseConnectionSettings connectionSettings) =>
        builder.EnableRetryOnFailure(
            connectionSettings.MaxRetryCount,
            TimeSpan.FromSeconds(connectionSettings.MaxRetryDelay), null);

    public static DatabaseConnectionSettings GetDatabaseConnectionSettings(
        this IConfiguration configuration,
        string connectionSettingsSectionName = DatabaseConnectionSettings.DatabaseConnectionSettingsDefaultSectionName) =>
        configuration.GetSection(connectionSettingsSectionName).Get<DatabaseConnectionSettings>();

    public static DbContextOptionsBuilder<TDbContext> GetDbContextOptionsBuilder<TDbContext>(this IDatabaseConnectionSettings connectionSettings, string connectionString)
        where TDbContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TDbContext>();
        switch (connectionSettings.DatabaseType)
        {
            case DatabaseType.MySql:
                builder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlDbContextOptionsBuilder => mySqlDbContextOptionsBuilder.EnableRetryOnFailure(connectionSettings));
                break;
            case DatabaseType.MsSql:
                builder.UseSqlServer(
                    connectionString,
                    sqlServerDbContextOptionsBuilder => sqlServerDbContextOptionsBuilder.EnableRetryOnFailure(connectionSettings));
                break;
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(
                    connectionString,
                    npgsqlDbContextOptionsBuilder => npgsqlDbContextOptionsBuilder.EnableRetryOnFailure(connectionSettings));
                break;
            default: throw new InvalidOperationException($"Data base type '{connectionSettings.DatabaseType}' is not supported.");
        }

        return builder;
    }
}