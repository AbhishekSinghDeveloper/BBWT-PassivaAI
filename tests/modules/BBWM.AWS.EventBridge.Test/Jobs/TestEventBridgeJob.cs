using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.AWS.EventBridge.Test.Jobs;

// This job's RunAsync method is mocked so we can verify it's called
public class TestEventBridgeJob : IEventBridgeJob
{
    public const string JOB_ID = "TestEventBridgeJobId";

    public const string JOB_DESCRIPTION = "This is a test event bridge job.";

    public static IEventBridgeJobMetadata<TestEventBridgeJob> Metadata = new MockMetadata<TestEventBridgeJob>(JOB_ID, JOB_DESCRIPTION, null);

    public virtual Task RunAsync(
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
