using BBWM.Core.Data;
using BBWM.Core.Membership;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BBWT.Data.MySQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBBWTMySQLDataContext(
        this IServiceCollection services,
        DatabaseConnectionSettings connectionSettings,
        string connectionString,
        Action<IdentityOptions> identityOptions,
        Action<WarningsConfigurationBuilder> configureContextWarningsBuilder = default)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options
                .UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    builder => builder.EnableRetryOnFailure(connectionSettings));

            if (configureContextWarningsBuilder != default)
                options.ConfigureWarnings(configureContextWarningsBuilder);
        });

        services.AddScoped<IDataContext, DataContext>();
        services.AddScoped<IDbContext, DataContext>();

        // try find a way to register the below in a single place
        services.AddMySqlSignInManager<DataContext>(identityOptions);

        // resolve IDataContext here
        return services;
    }
}
