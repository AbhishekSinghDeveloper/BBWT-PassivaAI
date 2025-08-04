using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryRuleTypeConfiguration : IEntityTypeConfiguration<QueryRuleType>
{
    public void Configure(EntityTypeBuilder<QueryRuleType> builder)
    {
        builder.ToTable("ReportingQueryRuleTypes");
    }
}
