using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class FilterControlConfiguration : IEntityTypeConfiguration<FilterControl>
{
    public void Configure(EntityTypeBuilder<FilterControl> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        builder.Property(x => x.HintText).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ExtraSettings).HasColumnType("text");

        builder.ToTable("ReportingFilterControls");
    }
}
