using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class AllowedIpConfiguration : IEntityTypeConfiguration<AllowedIp>
{
    public void Configure(EntityTypeBuilder<AllowedIp> builder)
    {
        builder.ToTable("AllowedIp");
        builder.HasMany(x => x.AllowedIpRoles);
        builder.HasMany(x => x.AllowedIpUsers);
    }
}
