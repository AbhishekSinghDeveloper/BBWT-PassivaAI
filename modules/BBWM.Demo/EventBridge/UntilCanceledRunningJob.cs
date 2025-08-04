using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.Demo.EventBridge;

internal sealed class UntilCanceledRunningJobMetadata : IEventBridgeJobMetadata<UntilCanceledRunningJob>
{
    public string JobId => "UntilCanceledRunningJob";

    public string JobDescription =>
        "Demo Event Bridge job. This job will run forever. It will, in a timely manner, " +
        "check whether a cancelation request have been made in order to terminate execution. " +
        "It will also allow us to see some jobs in the History -> Processing tab.";

    public List<JobParameterInfo> Parameters => new()
    {
        new()
        {
            Name = "Dummy Param",
            Required = false
        }
    };
}

public class UntilCanceledRunningJob : IEventBridgeJob
{
    public async Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(500, ct);
        }
    }
}
