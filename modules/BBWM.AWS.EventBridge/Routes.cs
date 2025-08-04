using BBWM.Core.Web;

namespace BBWM.AWS.EventBridge;

public static class Routes
{
    public static readonly Route Jobs = new("/app/aws-event-bridge/jobs", "Jobs");
    public static readonly Route History = new("/app/aws-event-bridge/history", "History");
    public static readonly Route Tech = new("/app/aws-event-bridge/tech", "Tech");
}
