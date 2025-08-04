using BBWM.Core.Data;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.Data;

public class DataContextModuleLinkage : IDataContextModuleLinkage
{
    public IServiceCollection AddDataContext(IServiceCollection services, IConfiguration configuration, DatabaseConnectionSettings defaultConnectionSettings)
    {
        var extraParamsFactory = new Func<IServiceProvider, IEnumerable<object>>(
            (serviceProvider) => new object[] { serviceProvider.GetService<IDbServices>() });

        switch (defaultConnectionSettings.DatabaseType)
        {
            case DatabaseType.MsSql:
                services.AddDbContext<IDemoDataContext, SqlServerDemoDataContext>(
                    defaultConnectionSettings.GetDbContextOptionsBuilder<SqlServerDemoDataContext>(
                        configuration.GetConnectionString("DemoConnection")).Options,
                    extraParamsFactory);
                break;
            case DatabaseType.MySql:
                services.AddDbContext<IDemoDataContext, MySqlDemoDataContext>(
                    defaultConnectionSettings.GetDbContextOptionsBuilder<MySqlDemoDataContext>(
                        configuration.GetConnectionString("DemoMySqlConnection")).Options,
                    extraParamsFactory);
                break;
            default: throw new Exception($"Demo module: Data base type '{defaultConnectionSettings.DatabaseType}' is not supported.");
        };

        return services;
    }

    public Type GetPrivateDataContextType() => typeof(IDemoDataContext);
}
