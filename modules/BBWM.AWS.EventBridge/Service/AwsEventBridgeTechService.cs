using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Data;

namespace BBWM.AWS.EventBridge.Service;

public class AwsEventBridgeTechService : IAwsEventBridgeTechService
{
    private readonly IDbContext context;

    public AwsEventBridgeTechService(IDbContext context)
    {
        this.context = context;
    }

    public ClearTablesResultDTO ClearEventBridgeTablesAsync()
    {
        return new ClearTablesResultDTO
        {
            JobsDeleted = context.Set<EventBridgeJob>().DeleteFromQuery(),
            HistoryDeleted = context.Set<EventBridgeJobHistory>().DeleteFromQuery(),
            RunningDeleted = context.Set<EventBridgeRunningJob>().DeleteFromQuery()
        };
    }
}
