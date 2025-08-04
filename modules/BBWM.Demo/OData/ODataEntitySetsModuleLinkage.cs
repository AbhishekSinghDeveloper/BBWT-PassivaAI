using BBWM.Core.Web.OData;
using BBWM.Demo.Northwind.Model;

using Microsoft.OData.ModelBuilder;

namespace BBWM.Demo.OData;

public class ODataEntitySetsModuleLinkage : IODataEntitySetsModuleLinkage
{
    public void AddEntitySets(ODataConventionModelBuilder builder, IServiceProvider provider = default)
    {
        builder.EntitySet<Customer>("Customers");
        builder.EntitySet<Order>("Orders");
    }
}
