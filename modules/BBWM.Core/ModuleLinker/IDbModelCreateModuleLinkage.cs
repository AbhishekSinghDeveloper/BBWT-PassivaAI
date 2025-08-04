using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.ModuleLinker;

/// <summary>
/// Use this interface in a project/BBWM-module code to collect your entities configurations for the
/// main db context of the application. The method of the interface is called by OnModelCreating of
/// the main data context.
/// NOTE! It's only needed when your entities are a part of the main context's database. In case if you use
/// a separate context/database, you should register entities configurations in the separate db context.
/// </summary>
/// <remarks>
/// <i>Example code of the method which collects all entities configurations of the module's assembly:
/// <code>
/// public void OnModelCreating(ModelBuilder builder)
/// {
///     builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
/// }
/// </code></i>
/// </remarks>
public interface IDbModelCreateModuleLinkage
{
    void OnModelCreating(ModelBuilder builder);
}

