namespace BBWM.Scheduler.DTO;
public class ServerInfoDTO
{
    public string ServerName { get; set; }
    public int Workers { get; set; }
    public string Queues { get; set; }
    public DateTime Started { get; set; }
    public DateTime Heartbeat { get; set; }
    public string StartedFormatted { get; set; }
    public string HeartbeatFormatted { get; set; }
}

