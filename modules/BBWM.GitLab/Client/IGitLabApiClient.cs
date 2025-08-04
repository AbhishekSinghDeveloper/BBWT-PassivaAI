namespace BBWM.GitLab.Client;

public interface IGitLabApiClient
{
    public IGitLabApiClient Setup(string gitLabApiUrl, string privateToken, string projectId);
    public bool BranchExists(string branchName);
    public bool MergeRequestExists(string branchName, string targetBranch);
    public void CreateBranch(string branchName, string branchNameFrom);
    public HttpResponseMessage CreateCommit(Commit commit);
    public void CreateMergeRequest(MergeRequest mergeRequest);
    public bool FileExists(string branchName, string filePath);
    public FileData GetFile(string branchName, string filePath);
}
