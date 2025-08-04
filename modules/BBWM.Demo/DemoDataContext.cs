using BBWM.Core.Audit;
using BBWM.Core.Services;
using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;
using System.Reflection;
using DemoFile = BBWM.Demo.Guidelines.File;

namespace BBWM.Demo;

public class DemoDataContext : AuditableDbContextCore, IDemoDataContext
{
    public DemoDataContext(DbContextOptions options, IDbServices dbServices) : base(options, dbServices)
    {
    }


    public DbSet<DemoFile> Files { get; set; }

    // Northwind
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
