using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.ModuleLinker;


public interface IInitialDataModuleLinkage
{
    /// <summary>
    /// Initialize/sync database data of the module on application start.
    /// </summary>
    /// <param name="includingOnceSeededData">When True means that application durring current start up
    /// is performing the one-off initialization of data that supposed to be seeded only once ever.
    /// For example, you may seed demo users once, but then that users can be removed from app UI and
    /// the app should NOT re-create them on further start up.
    /// </param>
    Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData);
}