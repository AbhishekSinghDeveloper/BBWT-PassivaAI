using BBWM.Core.DTO;

namespace BBWM.LoadingTime;

public class LoadingTimeDTO : IDTO
{
    public int Id { get; set; }

    /// <summary>
    /// loading time
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    /// page load
    /// </summary>
    public string Route { get; set; }

    public string UserAgent { get; set; }

    public string Account { get; set; }

    public DateTime DateTime { get; set; }
}
