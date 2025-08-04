namespace BBWM.Scheduler.DTO;
public class HourlyJobDTO
{
    public int Hour { get; set; }
    public int Failed { get; set; }
    public int Deleted { get; set; }
    public int Succeeded { get; set; }
}

