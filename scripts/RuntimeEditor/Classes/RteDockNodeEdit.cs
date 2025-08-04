using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeEditor.Classes;

public class RteDockNodeEdit
{
    public string Node { get; set; }
    public List<AttrValue> Attrs { get; set; }
    public DockTo Dock { get; set; }
    public DockTo PreviousDock { get; set; }
    public DockNodeAction Action { get; set; }

    private const string dockAttr = "dock";
    private const string previousDockAttr = "previous-dock";

    public static RteDockNodeEdit Parse(RteEdit[] edits)
    {
        var infos = edits
            .Select(o => new Tuple<RteEditAttrInfo, string>(RteEditAttrInfo.Parse(o.Attr), o.Value))
            .Where(o => o.Item1.IsNewNode)
            .ToList();

        if (!infos.Any()) return null;

        var parentNodeInfo = infos.Find(o => o.Item1.Attr == null);
        infos.Remove(parentNodeInfo);

        var result = new RteDockNodeEdit
        {
            Node = infos.First().Item1.Node
        };

        result.Action = DockNodeAction.Create;
        if (parentNodeInfo == null)
        {
            result.Action = DockNodeAction.Update;
        }
        else if (parentNodeInfo.Item2 == DockNodeAction.Delete.ToString())
        {
            result.Action = DockNodeAction.Delete;
        }

        var prevDockInfo = infos.Find(o => o.Item1.Attr == previousDockAttr);
        infos.Remove(prevDockInfo);
        result.PreviousDock = ToDockTo(prevDockInfo?.Item2) ?? DockTo.Right;

        var dockInfo = infos.Find(o => o.Item1.Attr == dockAttr);
        infos.Remove(dockInfo);
        result.Dock = ToDockTo(dockInfo?.Item2) ?? result.PreviousDock;

        // All the rest infos are node attrs
        result.Attrs = infos.ConvertAll(o => new AttrValue(o.Item1.Attr, o.Item2));

        return result;
    }

    private static DockTo? ToDockTo(string s)
    {
        s = s?.ToLowerInvariant();
        if (s == DockTo.Left.ToString().ToLowerInvariant()) return DockTo.Left;
        if (s == DockTo.Right.ToString().ToLowerInvariant()) return DockTo.Right;
        return null;
    }
}