using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Encodings.Web;

namespace BBWM.SsoProviders;

public class LinkedInServicesModuleLinkage : IAuthenticationModuleLinkage, IConfigureModuleLinkage, IInitialDataModuleLinkage
{
    public void Register(AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration configuration,
        Func<IServiceProvider> getServicesProvider)
    {
        authBuilder
            .AddLinkedIn(options =>
            {
                try
                {
                    var serviceProvider = getServicesProvider();

                    var settingsService = serviceProvider.GetService<ISettingsService>();

                    var linkedInSsoSettings = settingsService.GetSettingsSection<LinkedInSsoSettings>();

                    options.ClientId = string.IsNullOrEmpty(linkedInSsoSettings.ClientId) ? "null" : linkedInSsoSettings.ClientId;
                    options.ClientSecret = string.IsNullOrEmpty(linkedInSsoSettings.ClientSecret) ? "null" : linkedInSsoSettings.ClientSecret;
                }
                catch (Exception ex)
                {
                    options.ClientId = "null";
                    options.ClientSecret = "null";
                    ModuleLinker.AddCommonException(ex);
                }
                finally
                {
                    options.Events = new OAuthEvents
                    {
                        OnRemoteFailure = ctx =>
                        {
                            ctx.Response.Redirect(
                                $"/SsoProvider/external-login?remoteError={UrlEncoder.Default.Encode(ctx.Failure.Message)}");
                            ctx.HandleResponse();
                            return Task.FromResult(0);
                        }
                    };
                }
            });
    }

    public void ConfigureModule(IApplicationBuilder app) => app.RegisterSection<LinkedInSsoSettings>("LinkedInSsoSettings");

    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
        {
                new SettingsDTO { Value = service.GetSettingsSection<LinkedInSsoSettings>() ?? new LinkedInSsoSettings() },
            };

        return service.Save(appSettings);
    }
}
