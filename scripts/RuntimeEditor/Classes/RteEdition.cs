namespace RuntimeEditor.Classes;

public class RteEdition
{
    public RteNodeEdits[] Edits = new RteNodeEdits[] { };
}

public class RteEditionUpdate
{
    public RteEditionSumbmitUser SubmittedBy;
    public RteNodeEdits[] Edits = new RteNodeEdits[] { };
}

public class RteEditionSumbmitUser
{
    public string Name;
    public string Email;
}

public class RteNodeEdits
{
    public string RteId;
    public RteEdit[] Edits;
}

public class RteEdit
{
    public string Value;
    public string Attr;
}

public enum DockTo { None, Left, Right }

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
        IsNewNode ? string.Format("<{0}{1}>", Node, Attr == null ? "" : "." + Attr) : Attr;
}