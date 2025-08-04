using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.DbDoc.ModelConfiguration;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.DatabaseSource)
            .WithOne(x => x.Folder)
            .HasForeignKey<Folder>(x => x.DatabaseSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("DbDocFolders");
    }
}
