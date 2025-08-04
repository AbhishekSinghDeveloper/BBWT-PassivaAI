using BBWM.AWS.EventBridge.DTO;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IEventBridgeJob
{
    Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct = default);
}
