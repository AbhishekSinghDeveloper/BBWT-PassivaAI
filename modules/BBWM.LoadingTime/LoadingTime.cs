using BBWM.Core.Data;

namespace BBWM.LoadingTime;

public class LoadingTime : IEntity
{
    public int Id { get; set; }

    public int Time { get; set; }

    public string Route { get; set; }

    public string UserAgent { get; set; }

    public string Account { get; set; }

    public DateTime DateTime { get; set; }
}
