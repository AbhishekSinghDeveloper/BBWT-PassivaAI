using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class UserOrganizationGroupConfiguration : IEntityTypeConfiguration<UserOrganizationGroup>
{
    public void Configure(EntityTypeBuilder<UserOrganizationGroup> builder)
    {
        _ = builder.ToTable("UserOrganizationGroups");

        _ = builder.HasKey(uog => new { uog.UserId, uog.OrganizationId, uog.GroupId });

        _ = builder
            .HasOne(uog => uog.UserOrganization)
            .WithMany(uo => uo.Groups)
            .HasForeignKey(uog => new { uog.UserId, uog.OrganizationId })
            .OnDelete(DeleteBehavior.Cascade);

        _ = builder.HasOne(uog => uog.Group)
            .WithMany(g => g.UserOrganizationGroups)
            .HasForeignKey(uog => uog.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
