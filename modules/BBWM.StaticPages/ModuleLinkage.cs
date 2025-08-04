using Autofac;
using BBWM.Core;
using BBWM.Core.Autofac;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Menu;
using BBWM.Menu.DTO;
using Microsoft.EntityFrameworkCore;

namespace BBWM.StaticPages;

public class ModuleLinkage :
    IMenuModuleLinkage,
    IDependenciesModuleLinkage,
    IDbModelCreateModuleLinkage

{
    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO(Routes.StaticPages, "language"));

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<StaticPage>();
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();
    }
}

public class RouteRolesModule : IRouteRolesModule
{
    private readonly IDataService _dataService;

    public RouteRolesModule(IDataService dataService)
        => _dataService = dataService;

    public List<PageInfoDTO> GetRouteRoles()
    {
        var pageRoutes = new List<PageInfoDTO>
            {
                new PageInfoDTO(Routes.StaticPages, Core.Roles.SystemAdminRole),
                new PageInfoDTO(Routes.StaticPagesDetails, Core.Roles.SystemAdminRole),
            };

        foreach (var page in _dataService.GetAll<StaticPage, StaticPageDTO>().Result)
        {
            pageRoutes.Add(new PageInfoDTO($"/app/static/{page.Alias}", page.Heading,
                new List<string> { AggregatedRoles.Authenticated }));
        }

        return pageRoutes;
    }
}
