using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWT.Tests.modules.BBWM.AWS.EventBridge.Test.Jobs;

public class InvalidParamsJob : IEventBridgeJob
{
    public const string JOB_ID = "InvalidParamsJob";

    public static IEventBridgeJobMetadata<InvalidParamsJob> Metadata
        = new MockMetadata<InvalidParamsJob>(
           JOB_ID, null, new() { new JobParameterInfo { Name = "MyParam" }, new JobParameterInfo { Name = "MyParam" }, });

    public virtual Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
