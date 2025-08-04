using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");
        builder.Property(o => o.KeyHandle).IsRequired();
        builder.Property(o => o.PublicKey).IsRequired();
        builder.Property(o => o.AttestationCert).IsRequired();
    }
}
