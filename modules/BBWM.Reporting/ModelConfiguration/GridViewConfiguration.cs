using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class GridViewConfiguration : IEntityTypeConfiguration<GridView>
{
    public void Configure(EntityTypeBuilder<GridView> builder)
    {
        builder
            .HasOne(x => x.DefaultSortColumn)
            .WithOne()
            .HasForeignKey<GridView>(x => x.DefaultSortColumnId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("ReportingGridViews");
    }
}
