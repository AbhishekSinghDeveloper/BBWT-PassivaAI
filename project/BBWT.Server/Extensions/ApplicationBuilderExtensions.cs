using BBWM.Maintenance;
using BBWM.SystemSettings;

namespace BBWT.Server.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void RegisterBbwtSettings(this IApplicationBuilder app)
    {
        // System settings
        app.RegisterSystemSettings();

        // Maintenance
        app.RegisterMaintenanceSettings();
    }
}
