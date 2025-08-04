using BBWM.AWS.EventBridge.DTO;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IEventBridgeJobErrorHandler
{
    void HandleJobFailure(
        string ruleId,
        IEventBridgeJob sourceJob,
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        Exception error);

    void HandleJobCancelation(
        string ruleId,
        IEventBridgeJob sourceJob,
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        bool canceledByUser);
}
