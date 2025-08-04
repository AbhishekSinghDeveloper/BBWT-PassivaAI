using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class GridColumnViewConfiguration : IEntityTypeConfiguration<GridColumnView>
{
    public void Configure(EntityTypeBuilder<GridColumnView> builder)
    {
        builder.HasOne(x => x.ColumnViewMetadata)
            .WithOne(x => x.GridColumnView)
            .HasForeignKey<GridColumnView>(x => x.ColumnViewMetadataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("DbDocGridColumnViews");
    }
}
