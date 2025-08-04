using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Default Identity's auto generating of the ID property is disabled because we need to manually control
        // IDs to syncronize roles through all the app instances.
        // Roles are syncronized via Gitlab data sending: https://wiki.bbconsult.co.uk/display/BLUEB/Sending+Data+back+to+GitLab
        builder.Property(x => x.Id).HasMaxLength(255).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.NormalizedName).HasMaxLength(256);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(256);
    }
}
