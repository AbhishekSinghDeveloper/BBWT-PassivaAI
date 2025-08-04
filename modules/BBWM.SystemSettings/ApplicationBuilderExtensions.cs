using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.SystemSettings;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder RegisterSection<T>(this IApplicationBuilder app, string sectionName)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var settingsSectionService = scope.ServiceProvider.GetRequiredService<ISettingsSectionService>();
        settingsSectionService.RegisterSection<T>(sectionName);

        return app;
    }

    public static void RegisterSystemSettings(this IApplicationBuilder app)
        => app
            .RegisterSection<AppInitializationSettings>("AppInitializationSettings")
            .RegisterSection<ProjectSettings>("ProjectSettings")
            .RegisterSection<PwaSettings>("PwaSettings");
}
