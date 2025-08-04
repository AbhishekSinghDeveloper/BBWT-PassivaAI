using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryTableColumnConfiguration : IEntityTypeConfiguration<QueryTableColumn>
{
    public void Configure(EntityTypeBuilder<QueryTableColumn> builder)
    {
        builder.Property(x => x.SourceColumnId).HasMaxLength(500).IsRequired();

        builder.ToTable("ReportingQueryTableColumns");
    }
}
