namespace BBWM.Core.Membership.DTO;

public class RolesGitDataDTO
{
    public DateTimeOffset LastUpdated { get; set; }
    public List<RoleMetadataDTO> Roles { get; set; } = new List<RoleMetadataDTO>();
}
