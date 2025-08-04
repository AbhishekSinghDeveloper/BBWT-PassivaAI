namespace BBWM.Scheduler.DTO;
public class DailyJobDTO
{
    public DateTime Date { get; set; }
    public int Failed { get; set; }
    public int Deleted { get; set; }
    public int Succeeded { get; set; }
}
