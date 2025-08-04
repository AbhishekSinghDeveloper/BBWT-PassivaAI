using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class ColumnMetadataConfiguration : IEntityTypeConfiguration<ColumnMetadata>
{
    public void Configure(EntityTypeBuilder<ColumnMetadata> builder)
    {
        builder.HasOne(x => x.Table)
            .WithMany(x => x.Columns)
            .HasForeignKey(x => x.TableId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ColumnType)
            .WithMany()
            .HasForeignKey(x => x.ColumnTypeId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ValidationMetadata)
            .WithOne()
            .HasForeignKey<ColumnMetadata>(x => x.ValidationMetadataId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ViewMetadata)
            .WithOne()
            .HasForeignKey<ColumnMetadata>(x => x.ViewMetadataId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.ColumnId).IsRequired();

        builder.ToTable("DbDocColumnMetadata");
    }
}
