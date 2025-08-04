namespace BBWM.Scheduler.DTO;
public class PageResultDTO
{
    public List<JobRunDetailsDTO> Items { get; set; }
    public int Total { get; set; }
}

public class PageServerResultDTO
{
    public List<ServerInfoDTO> Items { get; set; }
    public int Total { get; set; }
}

