using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class ColumnViewMetadataConfiguration : IEntityTypeConfiguration<ColumnViewMetadata>
{
    public void Configure(EntityTypeBuilder<ColumnViewMetadata> builder) =>
        builder.ToTable("DbDocColumnViewMetadata");
}
