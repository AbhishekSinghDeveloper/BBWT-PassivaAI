using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Demo.Northwind.ModelConfiguration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasIndex(p => p.OrderDate);
        builder.HasIndex(p => p.RequiredDate);
        builder.HasIndex(p => p.ShippedDate);

        builder.HasOne(p => p.Customer)
               .WithMany(p => p.Orders)
               .HasForeignKey(p => p.CustomerId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Employee)
               .WithMany(p => p.Orders)
               .HasForeignKey(p => p.EmployeeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.OrderDate).HasColumnType("date");
        builder.Property(p => p.RequiredDate).HasColumnType("date");
        builder.Property(p => p.ShippedDate).HasColumnType("date");
    }
}
