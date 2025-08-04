using BBWM.Core.Data;
using BBWM.Core.Membership;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BBWT.Data.SqlServer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBBWTSqlServerDataContext(
        this IServiceCollection services,
        DatabaseConnectionSettings connectionSettings,
        string connectionString,
        Action<IdentityOptions> identityOptions,
        Action<WarningsConfigurationBuilder> configureContextWarningsBuilder = default)

    {
        services.AddDbContext<DataContext>(options =>
        {
            options
                .UseSqlServer(connectionString, builder => builder.EnableRetryOnFailure(connectionSettings));

            if (configureContextWarningsBuilder != default)
                options.ConfigureWarnings(configureContextWarningsBuilder);
        });

        services.AddScoped<IDataContext, DataContext>();
        services.AddScoped<IDbContext, DataContext>();

        services.AddSqlServerSignInManager<DataContext>(identityOptions);

        return services;
    }
}
