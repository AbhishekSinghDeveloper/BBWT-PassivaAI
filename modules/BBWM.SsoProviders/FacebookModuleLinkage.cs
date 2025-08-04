using BBWM.Core.ModuleLinker;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text.Encodings.Web;

namespace BBWM.SsoProviders;

public class FacebookModuleLinkage : IAuthenticationModuleLinkage, IConfigureModuleLinkage, IInitialDataModuleLinkage
{
    public void Register(AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration configuration,
        Func<IServiceProvider> getServicesProvider)
    {
        authBuilder
            .AddFacebook(options =>
            {
                try
                {
                    var serviceProvider = getServicesProvider();

                    var settingsService = serviceProvider.GetService<ISettingsService>();

                    var facebookSsoSettings = settingsService.GetSettingsSection<FacebookSsoSettings>();

                    options.AppId = string.IsNullOrEmpty(facebookSsoSettings.AppId)
                        ? "null"
                        : facebookSsoSettings.AppId;
                    options.AppSecret = string.IsNullOrEmpty(facebookSsoSettings.AppSecret)
                        ? "null"
                        : facebookSsoSettings.AppSecret;
                }
                catch (Exception ex)
                {
                    options.AppId = "null";
                    options.AppSecret = "null";
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

    public void ConfigureModule(IApplicationBuilder app) => app.RegisterSection<FacebookSsoSettings>("FacebookSsoSettings");

    public Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();

        var appSettings = new[]
        {
                new SettingsDTO { Value = service.GetSettingsSection<FacebookSsoSettings>() ?? new FacebookSsoSettings() },
            };

        return service.Save(appSettings);
    }
}
