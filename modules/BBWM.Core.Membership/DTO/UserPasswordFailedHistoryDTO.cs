using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

public class UserPasswordFailedHistoryDTO : IDTO
{
    public int Id { get; set; }
    public string email { get; set; }
    public DateTime failedDate { get; set; }
    public string IpAddress { get; set; }
}
