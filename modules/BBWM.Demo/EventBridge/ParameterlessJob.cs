using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.Demo.EventBridge;

internal sealed class ParameterlessJobMetadata : IEventBridgeJobMetadata<ParameterlessJob>
{
    public string JobId => "ParameterlessJob";

    public string JobDescription =>
        "Demo Event Bridge job. This job does nothing. It's just a proof of concept " +
        "to know how the UI behave when switching to a parameterless job at the " +
        "time of add/edit.";

    public List<JobParameterInfo> Parameters => null;
}

public class ParameterlessJob : IEventBridgeJob
{
    public Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct) => Task.CompletedTask;
}
