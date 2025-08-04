using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Title).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(int.MaxValue);

        builder.ToTable("ReportingSections");
    }
}
