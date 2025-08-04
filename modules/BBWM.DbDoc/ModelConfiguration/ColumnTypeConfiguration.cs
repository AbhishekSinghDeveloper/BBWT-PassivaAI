using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class ColumnTypeConfiguration : IEntityTypeConfiguration<ColumnType>
{
    public void Configure(EntityTypeBuilder<ColumnType> builder)
    {

        builder.HasOne(x => x.ValidationMetadata)
            .WithOne()
            .HasForeignKey<ColumnType>(x => x.ValidationMetadataId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ViewMetadata)
            .WithOne()
            .HasForeignKey<ColumnType>(x => x.ViewMetadataId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).IsRequired();

        builder.ToTable("DbDocColumnType");
    }
}
