namespace BBWM.Core.Membership.Interfaces;

public interface IRoleGitDataService
{
    Task UpdateRolesFromJson(CancellationToken cancellationToken = default);
    Task SendToGit(CancellationToken cancellationToken = default);
}
