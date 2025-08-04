using BBWM.AWS.EventBridge.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.AWS.EventBridge.ModelConfiguration;

public class RuleDetailEntityTypeConfiguration : IEntityTypeConfiguration<EventBridgeJob>
{
    public void Configure(EntityTypeBuilder<EventBridgeJob> builder)
    {
        builder.ToTable("EventBridgeJobs");

        builder.Property(j => j.JobId).IsRequired();
        builder.HasIndex(r => r.JobId);

        builder.HasIndex(j => j.RuleId).IsUnique();

        builder
            .Property(j => j.Parameters)
            .HasListToJsonConversion(
                @params => @params.Select(
                    param => new EventBridgeJobParameter
                    {
                        Name = param.Name,
                        Value = param.Value
                    }).ToList());
    }
}
