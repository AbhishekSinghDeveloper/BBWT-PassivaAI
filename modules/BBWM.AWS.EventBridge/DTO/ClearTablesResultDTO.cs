namespace BBWM.AWS.EventBridge.DTO;

public class ClearTablesResultDTO
{
    public int JobsDeleted { get; set; }

    public int HistoryDeleted { get; set; }

    public int RunningDeleted { get; set; }
}
