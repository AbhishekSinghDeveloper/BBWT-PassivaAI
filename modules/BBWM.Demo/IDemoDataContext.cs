using BBWM.Core.Data;
using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;

using DemoFile = BBWM.Demo.Guidelines.File;

namespace BBWM.Demo;

public interface IDemoDataContext : IDbContext
{
    // Northwind
    DbSet<Customer> Customers { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderDetails> OrderDetails { get; set; }
    DbSet<Employee> Employees { get; set; }

    DbSet<DemoFile> Files { get; set; }
}
