using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Demo.IdHashing.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.IdHashing;

public class ConfigureIdHashing : IConfigureModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();

        modelHashingService.ManualPropertyHashing<OrderHashingDTO, Order>(order => order.Id);
        modelHashingService.ManualPropertyHashing<SimpleOrderHashingDTO, Order>(order => order.Id);
        modelHashingService.ManualPropertyHashing<OrderDetailHashingDTO, Order>(detail => detail.Id);
        modelHashingService.ManualPropertyHashing<CustomerHashingDTO, Customer>(customer => customer.Id);
    }
}
