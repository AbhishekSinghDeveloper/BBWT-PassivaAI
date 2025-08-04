using BBWM.Core.Data;

namespace BBWM.Core.Membership.Model;

public class UserPasswordFailedHistory : IEntity
{
    public int Id { get; set; }

    public string email { get; set; }

    public DateTime failedDate { get; set; }

    public string IpAddress { get; set; }

}
