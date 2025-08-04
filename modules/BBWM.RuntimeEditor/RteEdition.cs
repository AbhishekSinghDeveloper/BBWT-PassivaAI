namespace BBWM.RuntimeEditor;

public class RteEdition
{
    public RteNodeEdits[] Edits { get; set; } = Array.Empty<RteNodeEdits>();
}

public class RteEditionUpdate
{
    public RteEditionSumbmitUser SubmittedBy { get; set; }
    public RteNodeEdits[] Edits { get; set; } = Array.Empty<RteNodeEdits>();
}

public class RteEditionSumbmitUser
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class RteNodeEdits
{
    public string RteId { get; set; }
    public RteEdit[] Edits { get; set; }
}

public class RteEdit
{
    public string Value { get; set; }
    public string Attr { get; set; }
}

public enum DockTo { Left, Right }

public enum DockNodeAction { Create, Update, Delete }

public class RteEditAttrInfo
{
    public string Attr { get; set; }
    public string Node { get; set; }
    public bool IsNewNode { get; set; }

    public static RteEditAttrInfo Parse(string s)
    {
        var result = new RteEditAttrInfo { };

        if (s.StartsWith("<"))
        {
            result.IsNewNode = true;
            var arr = s.TrimStart('<').TrimEnd('>').Split('.');
            result.Node = arr[0];
            if (arr.Length > 1)
                result.Attr = arr[1];
        }
        else
        {
            result.Attr = s;
        }

        return result;
    }

    public override string ToString() =>
        IsNewNode ? string.Format("<{0}{1}>", Node, Attr is null ? "" : "." + Attr) : Attr;
}
