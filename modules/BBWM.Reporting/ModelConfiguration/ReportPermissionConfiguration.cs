using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class ReportPermissionConfiguration : IEntityTypeConfiguration<ReportPermission>
{
    public void Configure(EntityTypeBuilder<ReportPermission> builder)
    {
        builder.HasKey(x => new { x.ReportId, x.PermissionId });

        builder.HasOne(x => x.Report)
            .WithMany(x => x.ReportPermissions)
            .HasForeignKey(x => x.ReportId);

        builder.ToTable("ReportingPermissions");
    }
}
