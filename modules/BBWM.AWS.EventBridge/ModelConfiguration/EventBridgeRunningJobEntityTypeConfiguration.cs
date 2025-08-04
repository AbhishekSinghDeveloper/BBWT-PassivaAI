using BBWM.AWS.EventBridge.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace BBWM.AWS.EventBridge.ModelConfiguration;

public class EventBridgeRunningJobEntityTypeConfiguration : IEntityTypeConfiguration<EventBridgeRunningJob>
{
    public void Configure(EntityTypeBuilder<EventBridgeRunningJob> builder)
    {
        builder.ToTable("EventBridgeRunningJobs");

        builder.Property(r => r.JobId).IsRequired();
        builder.HasIndex(r => r.JobId);

        builder.Property(r => r.StartTime).IsRequired();
        builder.HasIndex(r => r.StartTime);

        builder.HasIndex(r => r.RuleId);

        builder
            .Property(r => r.CancelationId)
            .HasValueGenerator<SequentialGuidValueGenerator>()
            .IsRequired();

        builder.HasIndex(r => r.CancelationId).IsUnique();
    }
}
