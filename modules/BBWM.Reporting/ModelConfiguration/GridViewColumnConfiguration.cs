using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class GridViewColumnConfiguration : IEntityTypeConfiguration<GridViewColumn>
{
    public void Configure(EntityTypeBuilder<GridViewColumn> builder)
    {
        builder.Property(x => x.Header).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ExtraSettings).HasColumnType("text");
        builder.Property(x => x.Footer).HasColumnType("text");

        builder.ToTable("ReportingGridViewColumns");
    }
}
