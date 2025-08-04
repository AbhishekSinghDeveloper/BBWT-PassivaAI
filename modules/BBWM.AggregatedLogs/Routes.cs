using BBWM.Core.Web;

namespace BBWM.AggregatedLogs;

public static class Routes
{
    public static readonly Route Logs = new("/app/logs", "Logs");
}