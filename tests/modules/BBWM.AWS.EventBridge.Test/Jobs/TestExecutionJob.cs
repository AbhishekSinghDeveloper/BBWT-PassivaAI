using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.AWS.EventBridge.Test.Jobs;

public class TestExecutionJob : TestJobBase, IEventBridgeJob
{
    public const string JOB_ID = "TestExecutionJobId";
    public const string JOB_DESCRIPTION = "This job is meant to facilitate testing the job execution wrapper";
    private readonly bool failExecution;

    public event Func<CancellationToken, Task> OnStart;

    public event Func<CancellationToken, Task> BeforeEnd;

    public static IEventBridgeJobMetadata<TestExecutionJob> Metadata = new MockMetadata<TestExecutionJob>(JOB_ID, JOB_DESCRIPTION, null);

    public TestExecutionJob(bool failExecution)
    {
        this.failExecution = failExecution;
    }

    public virtual async Task RunAsync(
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        CancellationToken cancellationToken = default)
    {
        await AwaitEventAsync(OnStart, cancellationToken);

        await AwaitEventAsync(BeforeEnd, cancellationToken);

        Finish();

        if (failExecution)
        {
            throw new Exception(JOB_ID);
        }
    }

    private static async Task AwaitEventAsync(Func<CancellationToken, Task> @event, CancellationToken cancellationToken)
    {
        if (@event is not null)
        {
            await @event(cancellationToken);
        }
    }
}
