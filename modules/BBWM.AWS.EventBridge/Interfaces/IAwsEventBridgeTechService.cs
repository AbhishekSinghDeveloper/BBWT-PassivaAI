using BBWM.AWS.EventBridge.DTO;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IAwsEventBridgeTechService
{
    ClearTablesResultDTO ClearEventBridgeTablesAsync();
}
