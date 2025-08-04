using Microsoft.AspNetCore.Routing;

namespace BBWM.Core.ModuleLinker;

public interface ISignalRModuleLinkage
{
    void MapHubs(IEndpointRouteBuilder routes);
}
