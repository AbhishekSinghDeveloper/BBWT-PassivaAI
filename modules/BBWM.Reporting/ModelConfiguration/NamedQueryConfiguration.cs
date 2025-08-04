using BBWM.Core.Membership.Model;
using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class NamedQueryConfiguration : IEntityTypeConfiguration<NamedQuery>
{
    public void Configure(EntityTypeBuilder<NamedQuery> builder)
    {
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedBy);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UpdatedBy);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(500).IsRequired();

        builder.ToTable("ReportingNamedQueries");
    }
}
