using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryFilterBindingConfiguration : IEntityTypeConfiguration<QueryFilterBinding>
{
    public void Configure(EntityTypeBuilder<QueryFilterBinding> builder)
    {
        builder.HasOne(x => x.QueryFilter)
               .WithMany(x => x.QueryFilterBindings)
               .HasForeignKey(x => x.QueryFilterId);
        builder.HasOne(x => x.FilterControl)
               .WithMany(x => x.QueryFilterBindings)
               .HasForeignKey(x => x.FilterControlId);
        builder.HasOne(x => x.MasterDetailSection)
               .WithMany(x => x.QueryFilterBindings)
               .HasForeignKey(x => x.MasterDetailSectionId);
        builder.HasOne(x => x.MasterDetailQueryTableColumn)
               .WithMany()
               .HasForeignKey(x => x.MasterDetailQueryTableColumnId);

        builder.ToTable("ReportingQueryFilterBindings");
    }
}
