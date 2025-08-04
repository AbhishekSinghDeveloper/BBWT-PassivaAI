using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;
using BBWM.GitLab.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.GitLab;

public class ModuleLinkage : IServicesModuleLinkage, IDependenciesModuleLinkage
{
    private const string gitlabSection = "GitLabSettings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(gitlabSection);
        if (section.Get<GitLabSettings>() is null)
            throw new EmptyConfigurationSectionException(gitlabSection);
        services.Configure<GitLabSettings>(section);
    }

    public void RegisterDependencies(ContainerBuilder builder)
        => builder.RegisterService<IGitLabApiClient, GitLabApiClient>();
}
