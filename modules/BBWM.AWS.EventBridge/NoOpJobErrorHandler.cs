using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;

namespace BBWM.AWS.EventBridge;

internal class NoOpJobErrorHandler : IEventBridgeJobErrorHandler
{
    public void HandleJobCancelation(
        string ruleId,
        IEventBridgeJob sourceJob,
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        bool canceledByUser)
    {
        // Handling is not supposed
    }

    public void HandleJobFailure(
        string ruleId,
        IEventBridgeJob sourceJob,
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        Exception error)
    {
        // Handling is not supposed
    }
}
