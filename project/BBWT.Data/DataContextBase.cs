using BBWM.Core.Audit;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BBWT.Data;

/// <summary>
/// This is the project-specific part of the main data context.
/// 
/// Explicitely add all project-specific DbSet definitions in this class.
/// E.g. <code>DbSet&lt;MyEntity&gt; MyEntities {get; set;}</code>
/// or use another appoach demonstrated in <see cref="DataContextBase.OnModelCreating(ModelBuilder)"/>.
/// Additional details in <see cref="IDataContext"/> interface annotations.
/// </summary>
public abstract class DataContextBase : AuditableIdentityDbContextCore<User, Role, string,
    IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>,
    IdentityUserToken<string>>, IDataContext
{

    protected DataContextBase(DbContextOptions options, IDbServices dbServices) : base(options, dbServices)
    {
    }

    /// <summary>
    /// A method to define all Entity Framework models configurations of the project.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Finding all implementation of IDbModelCreateModuleLinkage to collect entity configurations of project/modules.
        ModuleLinker.GetInstances<IDbModelCreateModuleLinkage>().ForEach(linker => linker.OnModelCreating(builder));

        #region Project-specific part of OnModelCreating
        // An example of registering "Invoice" model by Entity Framework
        // instead of explicit defining a DB set in the data context class:
        // builder.Entity<Invoice>();
        #endregion
    }
}
