namespace BBWM.AWS.EventBridge.Test.Jobs;

public abstract class TestJobBase : ITestJob
{
    private readonly object finishedLock = new object();
    private bool finished = false;

    public bool Finished
    {
        get
        {
            lock (finishedLock)
            {
                return finished;
            }
        }
    }

    protected void Finish()
    {
        lock (finishedLock)
        {
            finished = true;
        }
    }

    bool ITestJob.Finished { get => Finished; }
}
