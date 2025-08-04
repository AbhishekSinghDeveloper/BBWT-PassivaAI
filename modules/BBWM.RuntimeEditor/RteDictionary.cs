namespace BBWM.RuntimeEditor;

public enum RteDictionaryItemType { None, Html, TypeScript, CSharp };

public class RteDictionaryItemAttr
{
    public string Attr { get; set; }
    public string Value { get; set; }
}

public class RteDictionaryItem
{
    public string RteId { get; set; }
    public RteDictionaryItemType Type { get; set; }
    public string Phrase { get; set; }
    public RteDictionaryItemAttr[] Attrs { get; set; }
}

public class RteDictionary
{
    public RteDictionaryItem[] Items { get; set; }
}
