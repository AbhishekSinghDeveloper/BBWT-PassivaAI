using Microsoft.Extensions.Hosting;

namespace BBWM.Core.Tasks;

public class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedHostedService(IBackgroundTaskQueue taskQueue) => _taskQueue = taskQueue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _taskQueue.Dequeue(stoppingToken);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
