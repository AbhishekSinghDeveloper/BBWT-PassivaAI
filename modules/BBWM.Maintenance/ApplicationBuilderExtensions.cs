using BBWM.SystemSettings;

using Microsoft.AspNetCore.Builder;

namespace BBWM.Maintenance;

public static class ApplicationBuilderExtensions
{
    public static void RegisterMaintenanceSettings(this IApplicationBuilder app, string settingsSection = "MaintenanceSettings")
    {
        app.RegisterSection<MaintenanceSettings>(settingsSection);
    }
}
