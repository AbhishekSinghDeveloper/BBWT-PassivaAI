using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class ColumnValidationMetadataConfiguration : IEntityTypeConfiguration<ColumnValidationMetadata>
{
    public void Configure(EntityTypeBuilder<ColumnValidationMetadata> builder) =>
        builder.ToTable("DbDocColumnValidationMetadata");
}
