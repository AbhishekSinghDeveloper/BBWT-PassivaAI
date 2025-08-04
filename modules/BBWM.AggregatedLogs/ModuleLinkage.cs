using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Loggers;
using BBWM.Core.Membership;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Menu;
using BBWM.Menu.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.AggregatedLogs;

public class ModuleLinkage :
    IDependenciesModuleLinkage,
    IDataContextModuleLinkage,
    IDbCreateModuleLinkage,
    IServicesModuleLinkage,
    IMenuModuleLinkage
{
    public IServiceCollection AddDataContext(IServiceCollection services, IConfiguration configuration, DatabaseConnectionSettings connectionSettings)
    {
        var aggregatedLogsSettings = configuration.GetSection(AggregatedLogsSettings.AggregatedLogsSettingsDefaultSectionName).Get<AggregatedLogsSettings>()
                ?? new AggregatedLogsSettings();

        services.AddDbContext<ILogContext, LogContext>(
            aggregatedLogsSettings.GetDbContextOptionsBuilder<LogContext>(aggregatedLogsSettings.ConnectionString).Options);

        return services;
    }

    public Type GetPrivateDataContextType() => typeof(ILogContext);

    public void Create(IServiceScope serviceScope)
    {
        var logContext = serviceScope.ServiceProvider.GetService<ILogContext>();

        if (logContext != null)
        {
            logContext.Database.EnsureCreated();
        }
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IDataService<ILogContext>, DataService<ILogContext>>();
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();

        builder.RegisterService<IFileProvider, FileProvider>();
        builder.RegisterService<ILogLineParser, NcsaParser>();
        builder.RegisterService<IWebServerLogsService, WebServerLogsService>();

    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(WebServerLogsSettings.WebServerSettingsDefaultSectionName);
        var settings = section.Get<WebServerLogsSettings>();
        if (settings is null)
            throw new EmptyConfigurationSectionException(WebServerLogsSettings.WebServerSettingsDefaultSectionName);
        if (string.IsNullOrWhiteSpace(settings.FolderPath))
            throw new EmptyConfigurationSectionException($"{WebServerLogsSettings.WebServerSettingsDefaultSectionName}.{nameof(WebServerLogsSettings.FolderPath)}");
        if (string.IsNullOrWhiteSpace(settings.SourceName))
            throw new EmptyConfigurationSectionException($"{WebServerLogsSettings.WebServerSettingsDefaultSectionName}.{nameof(WebServerLogsSettings.SourceName)}");
        if (string.IsNullOrWhiteSpace(settings.AppName))
            throw new EmptyConfigurationSectionException($"{WebServerLogsSettings.WebServerSettingsDefaultSectionName}.{nameof(WebServerLogsSettings.AppName)}");
        services.Configure<WebServerLogsSettings>(section);
    }

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.TechnicalAdmin.Children.Add(new MenuDTO(Routes.Logs));

}
