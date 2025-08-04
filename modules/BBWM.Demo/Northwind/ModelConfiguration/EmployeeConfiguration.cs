using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Demo.Northwind.ModelConfiguration;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(p => p.RegistrationDate).HasColumnType("date").IsRequired(false);
    }
}
