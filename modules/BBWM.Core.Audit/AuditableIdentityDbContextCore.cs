using BBWM.Core.Data;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

public abstract class AuditableIdentityDbContextCore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    : IdentityDbContextCore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>, IDbContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    protected readonly IAuditWrapper _auditWrapper;


    protected AuditableIdentityDbContextCore(DbContextOptions options, IDbServices dbServices) : base(options, dbServices) =>

        _auditWrapper = dbServices.GetAuditWrapper();


    public override int SaveChanges()
    {
        if (_auditWrapper is null)
        {
            return base.SaveChanges();
        }

        OnBeforeSaveChanges();
        var result = base.SaveChanges();
        OnAfterSaveChanges().Wait();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_auditWrapper is null)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges();
        return result;
    }


    private void OnBeforeSaveChanges() =>
        _auditWrapper.OnBeforeSaveChanges(ChangeTracker.Entries().ToArray());

    private Task OnAfterSaveChanges() =>
        _auditWrapper.OnAfterSaveChanges();
}
