using BBWM.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.ModuleLinker;

public interface IDataContextModuleLinkage
{
    IServiceCollection AddDataContext(IServiceCollection services, IConfiguration configuration, DatabaseConnectionSettings connectionSettings);

    Type GetPrivateDataContextType() => null;
}
