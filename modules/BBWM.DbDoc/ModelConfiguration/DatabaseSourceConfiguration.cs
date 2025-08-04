using BBWM.Core.Membership.Model;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class DatabaseSourceConfiguration : IEntityTypeConfiguration<DatabaseSource>
{
    public void Configure(EntityTypeBuilder<DatabaseSource> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ContextId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.SchemaCode).HasMaxLength(100);

        builder.ToTable("DbDocDatabaseSource");
    }
}