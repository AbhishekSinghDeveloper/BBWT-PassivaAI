using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.SystemSettings;

public class ModuleLinkage :
    IInitialDataModuleLinkage,
    IServicesModuleLinkage,
    IDbModelCreateModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISettingsSectionService, SettingsSectionService>();
    }

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
        {
            new SettingsDTO { Value = service.GetSettingsSection<ProjectSettings>() ?? new ProjectSettings() },
            new SettingsDTO { Value = service.GetSettingsSection<PwaSettings>() ?? new PwaSettings() }
        };

        await service.Save(appSettings);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<AppSettings>();
    }
}
