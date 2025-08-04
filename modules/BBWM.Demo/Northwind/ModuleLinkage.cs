using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using BBWM.Demo.Northwind.Model;
using BBWM.Demo.Northwind.Services;
using BBWM.Demo.Northwind.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.Northwind;

public class ModuleLinkage : IRouteRolesModuleLinkage, IInitialDataModuleLinkage,
    ISignalRModuleLinkage
{
    void ISignalRModuleLinkage.MapHubs(IEndpointRouteBuilder routes) =>
        routes.MapHub<RandomDataHub>("/api/random-data");

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope)
    {
        return new List<PageInfoDTO>
            {
                new PageInfoDTO(Routes.Customers, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Employees, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Orders, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.OrderDetails, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Products, AggregatedRoles.Authenticated),
            };
    }
    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var randomDataService = serviceScope.ServiceProvider.GetService<IRandomDataService>();
        var context = serviceScope.ServiceProvider.GetService<IDemoDataContext>();

        if (!context.Set<Customer>().Any())
            await randomDataService.GenerateCustomers(100);

        if (!context.Set<Product>().Any())
            await randomDataService.GenerateProducts(100);

        if (!context.Set<Employee>().Any())
            await randomDataService.GenerateEmployees(100);

        if (!context.Set<Order>().Any())
            await randomDataService.GenerateOrders(100);
    }
}
