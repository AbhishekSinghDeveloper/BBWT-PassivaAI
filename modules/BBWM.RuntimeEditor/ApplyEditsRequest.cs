namespace BBWM.RuntimeEditor;

public class ApplyEditsRequest
{
    public string ChangeCommitName { get; set; }
    public string UserEmail { get; set; }
    public string UserName { get; set; }
    public string EditJsonFilesPath { get; set; }
    public RteEditionUpdate EditionUpdate { get; set; }
}
