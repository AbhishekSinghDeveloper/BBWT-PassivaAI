using Quartz;

namespace BBWM.Scheduler;
public class MyJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }
}
