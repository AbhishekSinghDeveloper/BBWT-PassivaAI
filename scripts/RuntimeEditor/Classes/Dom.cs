namespace RuntimeEditor.Classes;

public class NodeAttr
{
    public string Node { get; set; }
    public string Attr { get; set; }
    public NodeAttr(string node, string attr) { Node = node; Attr = attr; }
}

public class AttrValue
{
    public string Attr { get; set; }
    public string Value { get; set; }
    public AttrValue(string attr, string value) { Attr = attr; Value = value; }
}