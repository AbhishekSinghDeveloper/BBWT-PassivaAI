using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.ReCaptcha;

public class ModuleLinkage : IServicesModuleLinkage, IConfigureModuleLinkage, IInitialDataModuleLinkage
{
    private readonly string reCaptchaAppSettingsSection = "ReCaptchaSettings";
    private readonly string reCaptchaSysSettingsSection = "ReCaptchaSettings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(reCaptchaAppSettingsSection);

        var reCaptchaAppSettings = section.Get<ReCaptchaAppSettings>();

        if (reCaptchaAppSettings is null)
            throw new EmptyConfigurationSectionException(reCaptchaAppSettingsSection);
        if (string.IsNullOrWhiteSpace(reCaptchaAppSettings.ApiLink))
            throw new EmptyConfigurationSectionException($"{section}.{nameof(ReCaptchaAppSettings.ApiLink)}");
        if (string.IsNullOrWhiteSpace(reCaptchaAppSettings.SiteKey))
            throw new EmptyConfigurationSectionException($"{section}.{nameof(ReCaptchaAppSettings.SiteKey)}");
        if (string.IsNullOrWhiteSpace(reCaptchaAppSettings.SecretKey))
            throw new EmptyConfigurationSectionException($"{section}.{nameof(ReCaptchaAppSettings.SecretKey)}");
        if (reCaptchaAppSettings.AcceptableScore <= 0)
            throw new InvalidConfigurationSectionException($"{section}.{nameof(ReCaptchaAppSettings.AcceptableScore)}");

        services.Configure<ReCaptchaAppSettings>(section);
    }

    public void ConfigureModule(IApplicationBuilder app) => app.RegisterSection<ReCaptchaSettings>(reCaptchaSysSettingsSection);

    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
        {
                new SettingsDTO { Value = service.GetSettingsSection<ReCaptchaSettings>() }
            };

        return service.Save(appSettings);
    }
}
