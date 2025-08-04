using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryRuleConfiguration : IEntityTypeConfiguration<QueryRule>
{
    public void Configure(EntityTypeBuilder<QueryRule> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MySqlCodeTemplate).HasColumnType("text");
        builder.Property(x => x.MsSqlCodeTemplate).HasColumnType("text");

        builder.ToTable("ReportingQueryRules");
    }
}
