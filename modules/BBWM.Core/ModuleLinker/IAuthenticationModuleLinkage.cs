using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.ModuleLinker;

public interface IAuthenticationModuleLinkage
{
    void Register(AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration configuration, Func<IServiceProvider> getServicesProvider);
}
