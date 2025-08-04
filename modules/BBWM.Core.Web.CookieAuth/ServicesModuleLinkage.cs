using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Web.CookieAuth;

public class ServicesModuleLinkage : IServicesModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CookieAuthSettings.SectionName);
        var cookieAuthConfig = section.Get<CookieAuthSettings>();
        if (cookieAuthConfig is null)
            throw new EmptyConfigurationSectionException(CookieAuthSettings.SectionName);
        services.Configure<CookieAuthSettings>(section);

        services.ConfigureApplicationCookie(config =>
        {
            config.EventsType = typeof(CustomCookieAuthenticationEvents);
            config.Cookie.Name = cookieAuthConfig.CookieName;
            config.ExpireTimeSpan = TimeSpan.FromMinutes(cookieAuthConfig.ExpireTime == 0 ? 30 : cookieAuthConfig.ExpireTime);
            config.SlidingExpiration = true;
            config.LoginPath = new PathString(cookieAuthConfig.LoginPath);
            config.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        services.AddScoped<CustomCookieAuthenticationEvents>();
    }
}
