using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BBWM.Maintenance;

public class SignalRModuleLinkage : ISignalRModuleLinkage
{
    void ISignalRModuleLinkage.MapHubs(IEndpointRouteBuilder routes) =>
        routes.MapHub<MaintenanceHub>("/api/maintenance");
}
