namespace BBWM.Core.Tasks;

public interface IBackgroundTaskQueue
{
    void QueueBackgroundWorkItem(Func<CancellationToken, Task<object>> workItem);

    Task Dequeue(CancellationToken cancellationToken);

    void CancelWorkItem();

    T GetLastResult<T>() where T : class;
}
