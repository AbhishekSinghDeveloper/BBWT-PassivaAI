using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Maintenance;

public class DataModuleLinkage : IInitialDataModuleLinkage
{
    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
          {
                new SettingsDTO { Value = service.GetSettingsSection<MaintenanceSettings>() ?? new MaintenanceSettings() },
            };

        return service.Save(appSettings);
    }
}
