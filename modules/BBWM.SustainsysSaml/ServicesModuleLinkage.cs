using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;

namespace BBWM.SustainsysSaml;

public class ServicesModuleLinkage : IAuthenticationModuleLinkage
{
    private readonly string sustainsysSection = "SustainsysSaml";

    public void Register(AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration configuration,
        Func<IServiceProvider> getServicesProvider)
    {
        var config = configuration.GetSection(sustainsysSection).Get<SustainsysSamlSettings>();
        if (config is null
            || string.IsNullOrWhiteSpace(config.IdentifierUrl)
            || string.IsNullOrWhiteSpace(config.ProviderIdentifierUrl)
            || string.IsNullOrWhiteSpace(config.AppFederationMetadataUrl))
            throw new EmptyConfigurationSectionException(sustainsysSection);

        if (config.Enabled)
        {
            authBuilder
                .AddSaml2(options =>
                {
                    try
                    {
                        options.SPOptions.EntityId = new EntityId(config.IdentifierUrl);
                        options.IdentityProviders.Add(
                            new IdentityProvider(
                                new EntityId(config.ProviderIdentifierUrl), options.SPOptions)
                            {
                                MetadataLocation = config.AppFederationMetadataUrl
                            });
                    }
                    catch (Exception ex)
                    {
                        ModuleLinker.AddCommonException(ex);
                    }
                });
        }
    }
}
