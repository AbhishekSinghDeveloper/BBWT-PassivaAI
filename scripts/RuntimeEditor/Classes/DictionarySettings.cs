namespace RuntimeEditor.Classes;

public class DictionarySettings
{
    public string[] AllowedChildNodes { get; set; }
    public string[] InnerHtmlNodes { get; set; }
    public NodeAttr[] AllowedNodeAttrs { get; set; }
}