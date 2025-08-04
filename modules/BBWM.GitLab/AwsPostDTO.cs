namespace BBWM.GitLab;

public class AwsPostDTO
{
    public string projectId { get; set; }
    public string content { get; set; }
    public string sourceBranch { get; set; }
    public string function { get; set; }
    public string username { get; set; }
}
