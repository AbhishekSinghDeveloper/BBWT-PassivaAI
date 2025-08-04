using BBWM.AWS.EventBridge.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.AWS.EventBridge.ModelConfiguration;

public class EventBridgeJobHistoryEntityTypeConfiguration : IEntityTypeConfiguration<EventBridgeJobHistory>
{
    public void Configure(EntityTypeBuilder<EventBridgeJobHistory> builder)
    {
        builder.ToTable("EventBridgeJobsHistory");

        builder.Property(h => h.JobId).IsRequired();
        builder.HasIndex(h => h.JobId);

        builder.Property(h => h.StartTime).IsRequired();
        builder.HasIndex(h => h.StartTime);

        builder.Property(h => h.FinishTime).IsRequired();
        builder.HasIndex(h => h.FinishTime);

        builder.Property(h => h.CompletionStatus).IsRequired();

        builder.HasIndex(j => j.RuleId);

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
