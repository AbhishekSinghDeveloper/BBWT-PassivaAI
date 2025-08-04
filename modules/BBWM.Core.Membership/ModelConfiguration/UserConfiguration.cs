using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Core.Membership.ModelConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(x => x.AvatarImage).WithOne().HasForeignKey<User>(x => x.AvatarImageId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.InvitationToken).WithOne().HasForeignKey<User>(x => x.InvitationTokenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PasswordResetToken).WithOne().HasForeignKey<User>(x => x.PasswordResetTokenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EmailConfirmationToken).WithOne().HasForeignKey<User>(x => x.EmailConfirmationTokenId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Id).HasMaxLength(255).ValueGeneratedOnAdd();
        builder.Property(p => p.FirstName).HasMaxLength(35);
        builder.Property(p => p.LastName).HasMaxLength(35);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);
        builder.Property(p => p.AccountStatus).HasDefaultValue(AccountStatus.Active);
        builder.Property(x => x.SecurityStamp).HasMaxLength(256);
        builder.Property(x => x.RecoveryCode).HasMaxLength(256);
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(256);
    }
}
