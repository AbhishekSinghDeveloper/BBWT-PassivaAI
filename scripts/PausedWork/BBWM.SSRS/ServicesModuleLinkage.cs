using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

namespace BBWM.SSRS
{
    public class ServicesModuleLinkage: IServicesModuleLinkage
    {
        private const string ssrsSection = "SSRS";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(ssrsSection);
            if (section.Get<SsrsSettings>() == null)
                throw new EmptyConfigurationSectionException(ssrsSection);
            services.Configure<SsrsSettings>(section);
        }
    }
}