namespace BBWM.GitLab;

public interface IGitLabService
{
    Task<bool> Push(string function, string content, string username, CancellationToken cancellationToken = default);
    Task<bool> Run(string action, string content, string username, CancellationToken cancellationToken = default);
}
