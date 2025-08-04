using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class ReportRoleConfiguration : IEntityTypeConfiguration<ReportRole>
{
    public void Configure(EntityTypeBuilder<ReportRole> builder)
    {
        builder.HasKey(x => new { x.ReportId, x.RoleId });

        builder.HasOne(x => x.Report)
            .WithMany(x => x.ReportRoles)
            .HasForeignKey(x => x.ReportId);

        builder.ToTable("ReportingRoles");
    }
}
