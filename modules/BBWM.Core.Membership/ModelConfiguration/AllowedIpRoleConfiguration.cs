using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class AllowedIpRoleConfiguration : IEntityTypeConfiguration<AllowedIpRole>
{
    public void Configure(EntityTypeBuilder<AllowedIpRole> builder)
    {
        builder.ToTable("AllowedIpRoles");

        builder.HasOne(x => x.AllowedIp)
            .WithMany(x => x.AllowedIpRoles)
            .HasForeignKey(x => x.AllowedIpId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.AllowedIpRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
