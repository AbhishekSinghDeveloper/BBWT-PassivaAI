using BBWM.Core.Membership.Model;
using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedBy);
        builder.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedBy);
        builder.HasOne(x => x.PublishedReport).WithMany().HasForeignKey(x => x.PublishedReportId);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasMaxLength(500).IsRequired();
        builder.Property(p => p.UrlSlug).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Access).HasMaxLength(50);

        builder.ToTable("ReportingReports");
    }
}