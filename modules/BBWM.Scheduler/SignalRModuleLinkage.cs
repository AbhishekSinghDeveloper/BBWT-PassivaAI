using BBWM.Core.ModuleLinker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BBWM.Scheduler;
public class SignalRModuleLinkage : ISignalRModuleLinkage
{
    void ISignalRModuleLinkage.MapHubs(IEndpointRouteBuilder routes) =>
        routes.MapHub<JobStatusHub>("/api/scheduler/jobStatusHub");
}
