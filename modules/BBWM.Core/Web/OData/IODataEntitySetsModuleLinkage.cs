using Microsoft.OData.ModelBuilder;

namespace BBWM.Core.Web.OData;

public interface IODataEntitySetsModuleLinkage
{
    void AddEntitySets(ODataConventionModelBuilder builder, IServiceProvider provider = default);
}
