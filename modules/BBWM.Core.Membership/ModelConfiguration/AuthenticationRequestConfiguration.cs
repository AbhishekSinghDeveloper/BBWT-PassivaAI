using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class AuthenticationRequestConfiguration : IEntityTypeConfiguration<AuthenticationRequest>
{
    public void Configure(EntityTypeBuilder<AuthenticationRequest> builder)
    {
        builder.ToTable("AuthenticationRequests");
        builder.Property(o => o.Challenge).IsRequired();
        builder.Property(o => o.AppId).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Version).HasMaxLength(50).IsRequired();
    }
}
