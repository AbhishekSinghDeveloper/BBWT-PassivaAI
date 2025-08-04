using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class UserPasswordFailedHistoryConfiguration : IEntityTypeConfiguration<UserPasswordFailedHistory>
{
    public void Configure(EntityTypeBuilder<UserPasswordFailedHistory> builder) =>
        builder.ToTable("UserPasswordFailedHistory");
}
