using BBWM.Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Reflection;

using Z.EntityFramework.Plus;

namespace BBWM.Core.Data;

public class IdentityDbContextCore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> :
    IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>,
    IDbContext
   where TUser : IdentityUser<TKey>
   where TRole : IdentityRole<TKey>
   where TKey : IEquatable<TKey>
   where TUserClaim : IdentityUserClaim<TKey>
   where TUserRole : IdentityUserRole<TKey>
   where TUserLogin : IdentityUserLogin<TKey>
   where TRoleClaim : IdentityRoleClaim<TKey>
   where TUserToken : IdentityUserToken<TKey>
{
    protected readonly IMultiTenancyService _multiTenancyService;
    protected readonly int? _tenantId;


    protected IdentityDbContextCore(DbContextOptions options, IDbServices dbServices) : base(options)
    {
        _multiTenancyService = dbServices.GetMultiTenancyService();

        if (_multiTenancyService is not null)
        {
            _tenantId = _multiTenancyService.GetTenancyId();
            MultiTenancyFilter(_tenantId);
        }
    }


    public IImmutableDictionary<string, object> Features { get; private set; }


    public void FilterTenancyEntity<T>() where T : ITenantEntity
        => this.Filter<T>(x => x.Where(y => y.TenantId == _tenantId));


    public override int SaveChanges()
    {
        CheckTenantId();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        CheckTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CheckTenantId();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        CheckTenantId();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }


    private void MultiTenancyFilter(int? tenantId)
    {
        var method = typeof(IdentityDbContextCore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "FilterTenancyEntity");

        var entities = Model.GetEntityTypes();

        foreach (var entity in entities)
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entity.ClrType))
            {
                var genericMethod = method.MakeGenericMethod(entity.ClrType);
                genericMethod.Invoke(this, null);
            }
        }
    }

    private void CheckTenantId()
    {
        var addEntities = ChangeTracker.Entries<ITenantEntity>()
            .Where(p => p.State == EntityState.Added).ToList();

        foreach (var newEntity in addEntities)
        {
            newEntity.Entity.TenantId = _tenantId;
        }
    }
}
