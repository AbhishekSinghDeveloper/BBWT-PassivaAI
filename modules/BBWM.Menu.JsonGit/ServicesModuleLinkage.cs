using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Menu.JsonGit;

public class ServicesModuleLinkage : IServicesModuleLinkage, IDependenciesModuleLinkage
{
    private readonly string menuSection = "MenuSettings";
    private readonly string footerMenuSection = "FooterMenuSettings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var sectionMenu = configuration.GetSection(menuSection);
        if (sectionMenu.Get<MenuSettings>() is null)
            throw new EmptyConfigurationSectionException(menuSection);
        services.Configure<MenuSettings>(sectionMenu);

        var sectionFooterMenu = configuration.GetSection(footerMenuSection);
        if (sectionFooterMenu.Get<FooterMenuSettings>() is null)
            throw new EmptyConfigurationSectionException(footerMenuSection);
        services.Configure<FooterMenuSettings>(sectionFooterMenu);
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IMenuDataProvider, JsonGitMenuDataProvider>();
        builder.RegisterService<IFooterMenuDataProvider, JsonGitFooterMenuDataProvider>();
    }
}
