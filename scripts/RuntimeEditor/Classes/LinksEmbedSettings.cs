namespace RuntimeEditor.Classes;

public class LinksEmbedSettings
{
    public string[] AllowedNodes { get; set; }

    public string[] SkippedNodes { get; set; }

    public bool CleanupSkippedNodes { get; set; }

    public bool SkipEmptyNodes { get; set; }
    public string[] SkippedEmptyNodes { get; set; }

    public bool SkipParentNodes { get; set; }
    public string[] AllowedParentNodes { get; set; }
    public string[] AllowedChildNodes { get; set; }

    public bool SkipAttributeRequiredNodes { get; set; }
    public NodeAttr[] AttributeRequiredNodes { get; set; }
}