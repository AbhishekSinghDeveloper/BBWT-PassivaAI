using BBWM.Reporting.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryConfiguration : IEntityTypeConfiguration<Query>
{
    public void Configure(EntityTypeBuilder<Query> builder)
    {
        builder.ToTable("ReportingQueries");
    }
}
