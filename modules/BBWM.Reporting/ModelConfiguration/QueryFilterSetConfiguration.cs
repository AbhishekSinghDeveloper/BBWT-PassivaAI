using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryFilterSetConfiguration : IEntityTypeConfiguration<QueryFilterSet>
{
    public void Configure(EntityTypeBuilder<QueryFilterSet> builder)
    {
        builder.HasOne(x => x.Query)
            .WithMany(x => x.QueryFilterSets)
            .HasForeignKey(x => x.QueryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.ChildSets)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentQuery)
            .WithOne(x => x.RootFilterSet)
            .HasForeignKey<QueryFilterSet>(x => x.ParentQueryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("ReportingQueryFilterSets");
    }
}
