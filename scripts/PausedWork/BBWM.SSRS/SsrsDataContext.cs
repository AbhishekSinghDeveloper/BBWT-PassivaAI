using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace BBWM.SSRS
{
    public class SsrsDataContext : DbContext, ISsrsDataContext
    {
        public SsrsDataContext(DbContextOptions<SsrsDataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure default schema
        }

        public DbSet<Catalog> Catalog { get; set; }

        #region IDbContext requirements
        public BaseQueryFilter Filter<T>(Func<IQueryable<T>, IQueryable<T>> queryFilter, bool isEnabled = true) =>
            QueryFilterExtensions.Filter(this, queryFilter, isEnabled);

        public BaseQueryFilter Filter<T>(object key, Func<IQueryable<T>, IQueryable<T>> queryFilter, bool isEnabled = true) =>
            QueryFilterExtensions.Filter(this, key, queryFilter, isEnabled);

        public Dictionary<PropertyInfo, Type> FindKeys(Type type) => null;

        public int SaveOrUpdate() => throw new Exception("Not implemented");
        #endregion
    }
}