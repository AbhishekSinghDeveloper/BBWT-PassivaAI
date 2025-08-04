using BBWM.Core.Utils;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace BBWM.Reporting.ModelConfiguration;

public class QueryFilterConfiguration : IEntityTypeConfiguration<QueryFilter>
{
    public void Configure(EntityTypeBuilder<QueryFilter> builder)
    {
        builder.HasOne(x => x.QueryTableColumn)
            .WithMany()
            .HasForeignKey(x => x.QueryTableColumnId)
            .OnDelete(DeleteBehavior.Restrict);

        var converter = new ValueConverter<object, string>(
            v => JsonSerializer.Serialize(v, JsonSerializerOptionsProvider.Options),
            v => JsonSerializer.Deserialize<object>(v, JsonSerializerOptionsProvider.Options));

        builder.Property(x => x.Value).HasConversion(converter).HasColumnType("text");
        builder.Property(x => x.Value2).HasConversion(converter).HasColumnType("text");

        builder.ToTable("ReportingQueryFilters");
    }
}
