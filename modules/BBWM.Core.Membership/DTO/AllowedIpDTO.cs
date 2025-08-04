using BBWM.Core.DTO;

namespace BBWM.Core.Membership.DTO;

public class AllowedIpDTO : IDTO
{
    public int Id { get; set; }
    public string IpAddressFirst { get; set; }
    public string IpAddressLast { get; set; }

    public List<UserDTO> Users { get; set; }
    public List<RoleDTO> Roles { get; set; }
}
