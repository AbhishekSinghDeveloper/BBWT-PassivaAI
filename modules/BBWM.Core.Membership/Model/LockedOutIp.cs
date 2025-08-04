using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class LockedOutIp : IEntity
{
    public int Id { get; set; }
    public string IpAddress { get; set; }
    public DateTime LockoutEnd { get; set; }
}
