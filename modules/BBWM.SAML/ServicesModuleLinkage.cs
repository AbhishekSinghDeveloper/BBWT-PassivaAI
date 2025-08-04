using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BBWM.AutofacExtensions;
using BBWM.ModuleLinkage;
using BBWM.Core.Exceptions;

namespace BBWM.SAML
{
    public class ServicesModuleLinkage: IServicesModuleLinkage
    {
        private const string samlSection = "SAMLSettings";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(samlSection);
            if (section.Get<SamlSettings>() == null)
                throw new EmptyConfigurationSectionException(samlSection);
            services.Configure<SamlSettings>(section);
        }

        public void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterService<ISamlService, SamlService>();            
        }
    }
}