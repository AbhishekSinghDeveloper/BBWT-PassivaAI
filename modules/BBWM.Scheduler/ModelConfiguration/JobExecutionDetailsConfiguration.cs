using BBWM.Scheduler.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBWM.Scheduler.ModelConfiguration;

public class JobExecutionDetailsConfiguration : IEntityTypeConfiguration<JobRunDetails>
{
    public void Configure(EntityTypeBuilder<JobRunDetails> builder)
    {
        builder.ToTable("SchedulerJobRunDetails");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id)
               .ValueGeneratedOnAdd();

        builder.Property(j => j.JobName)
        .IsRequired();

        builder.Property(j => j.ExecutionTime)
            .IsRequired();

        builder.Property(j => j.Success)
            .IsRequired();

        builder.Property(j => j.Message)
            .IsRequired(false); // Optional

        builder.Property(j => j.Status)
            .IsRequired();

        builder.Property(j => j.JobGroup)
            .IsRequired(false);

        builder.Property(j => j.LastModified)
            .IsRequired();

        builder.Property(j => j.JobType)
            .IsRequired(false); // Optional

        builder.Property(j => j.TriggerType)
            .IsRequired(false); // Optional

        builder.Property(j => j.TriggerGroup)
            .IsRequired(false); // Optional

        builder.Property(j => j.Duration)
            .IsRequired(false); // Optional

        builder.Property(j => j.ServerName)
           .IsRequired(false); // Optional
    }
}
