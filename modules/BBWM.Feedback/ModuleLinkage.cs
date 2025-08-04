using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BBWM.Feedback;

public class ModuleLinkage : IInitialDataModuleLinkage, IConfigureModuleLinkage
{
    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();
        var hostingEnvironment = serviceScope.ServiceProvider.GetService<IWebHostEnvironment>();

        var appSettings = new[]
        {
                new SettingsDTO
                {
                    Value = service.GetSettingsSection<FeedbackSettings>() ??
                            new FeedbackSettings {
                                Enabled = !hostingEnvironment.IsDevelopment()
                            }
                }
            };

        return service.Save(appSettings);
    }

    public void ConfigureModule(IApplicationBuilder app) => app.RegisterSection<FeedbackSettings>("FeedbackSettings");
}
