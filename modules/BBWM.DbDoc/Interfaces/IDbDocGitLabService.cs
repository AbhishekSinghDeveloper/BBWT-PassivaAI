namespace BBWM.DbDoc.Interfaces;

public interface IDbDocGitLabService
{
    Task SendCurrentDbDocStateToGit(CancellationToken ct = default);
    Task SendCurrentDbDocStateToGit(bool isIninialization = false, CancellationToken ct = default);
}
