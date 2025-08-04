using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasOne(x => x.Branding)
            .WithOne(x => x.Organization)
            .HasForeignKey<Organization>(x => x.BrandingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
