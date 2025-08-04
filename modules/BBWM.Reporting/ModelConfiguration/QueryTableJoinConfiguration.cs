using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryTableJoinConfiguration : IEntityTypeConfiguration<QueryTableJoin>
{
    public void Configure(EntityTypeBuilder<QueryTableJoin> builder)
    {
        builder.HasOne(x => x.FromQueryTable)
            .WithMany()
            .HasForeignKey(x => x.FromQueryTableId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FromQueryTableColumn)
            .WithMany()
            .HasForeignKey(x => x.FromQueryTableColumnId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToQueryTable)
            .WithMany()
            .HasForeignKey(x => x.ToQueryTableId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToQueryTableColumn)
            .WithMany()
            .HasForeignKey(x => x.ToQueryTableColumnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("ReportingQueryTableJoins");
    }
}
