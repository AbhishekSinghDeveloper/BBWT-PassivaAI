using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryTableConfiguration : IEntityTypeConfiguration<QueryTable>
{
    public void Configure(EntityTypeBuilder<QueryTable> builder)
    {
        builder.Property(x => x.SourceTableId).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SourceCode).HasMaxLength(50);
        builder.Property(x => x.SelfJoinDbDocColumnId).HasMaxLength(500);
        builder.Property(x => x.Alias).HasMaxLength(500);

        builder.ToTable("ReportingQueryTables");
    }
}
