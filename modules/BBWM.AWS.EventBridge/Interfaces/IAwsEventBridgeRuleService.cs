using BBWM.AWS.EventBridge.DTO;
using BBWM.Core.Services;

namespace BBWM.AWS.EventBridge.Interfaces;

public interface IAwsEventBridgeRuleService :
    IEntityCreate<AwsEventBridgeRuleDTO>,
    IEntityUpdate<AwsEventBridgeRuleDTO>,
    IEntityDelete<string>,
    IEntityPage<AwsEventBridgeRuleDTO>
{
    Task<bool> RuleExistsAsync(string name, CancellationToken ct = default);
}
