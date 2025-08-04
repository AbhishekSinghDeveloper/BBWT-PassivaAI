using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Membership;

public interface IRouteRolesModuleLinkage
{
    List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope);
}

public interface IRouteRolesModule
{
    List<PageInfoDTO> GetRouteRoles();
}
