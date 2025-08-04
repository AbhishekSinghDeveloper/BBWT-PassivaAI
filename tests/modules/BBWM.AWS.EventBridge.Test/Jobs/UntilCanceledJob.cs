using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.AWS.EventBridge.Test.Jobs;

public class UntilCanceledJob : TestJobBase, IEventBridgeJob
{
    public const string JOB_ID = "UntilCanceledJob";
    public const string JOB_DESCRIPTION = "UntilCanceledJob";

    public static IEventBridgeJobMetadata<UntilCanceledJob> Metadata = new MockMetadata<UntilCanceledJob>(JOB_ID, JOB_DESCRIPTION, null);

    public virtual async Task RunAsync(
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Finish();
                cancellationToken.ThrowIfCancellationRequested();
            }
            await Task.Delay(100);
        }
    }
}
