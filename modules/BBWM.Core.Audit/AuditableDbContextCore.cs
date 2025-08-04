using BBWM.Core.Data;
using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit
{
    public class AuditableDbContextCore : DbContextCore
    {
        protected readonly IAuditWrapper _auditWrapper;


        protected AuditableDbContextCore(DbContextOptions options, IDbServices dbServices) : base(options) =>
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
}
