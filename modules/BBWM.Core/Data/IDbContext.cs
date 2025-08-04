using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBWM.Core.Data;

public interface IDbContext : IDisposable
{
    DatabaseFacade Database { get; }

    IModel Model { get; }

    DbSet<T> Set<T>() where T : class;

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry<TEntity> Entry<TEntity>(TEntity item) where TEntity : class;
}
