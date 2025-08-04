namespace BBWM.GitLab;

public class GitLabSettings
{
    /// <summary>
    /// Name of the GitLab project changes are applied to (e.g. "blueberry/bbwt3")
    /// </summary>
    public string ProjectId { get; set; }
    /// <summary>
    /// AWS API URL (to push changes via AWS lambda, e.g. https://90ebkemw8g.execute-api.eu-west-1.amazonaws.com/beta/gitpush)
    /// </summary>
    public string AwsApiUrl { get; set; }
    /// <summary>
    /// AWS API token (to push changes via AWS lambda)
    /// </summary>
    public string AwsApiToken { get; set; }
    /// <summary>
    /// Name of a GitLab branch where changes are applied to (e.g. "develop")
    /// </summary>
    public string Branch { get; set; }
    /// <summary>
    /// GitLab API URL (e.g. https://gitlab.bbconsult.co.uk/api/v4)
    /// </summary>
    public string GitLabApiUrl { get; set; }
    /// <summary>
    /// GitLab API token
    /// </summary>
    public string GitLabApiToken { get; set; }
}
