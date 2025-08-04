using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class TableMetadataConfiguration : IEntityTypeConfiguration<TableMetadata>
{
    public void Configure(EntityTypeBuilder<TableMetadata> builder)
    {
        builder.Property(x => x.TableId).IsRequired();

        builder.ToTable("DbDocTableMetadata");
    }
}
