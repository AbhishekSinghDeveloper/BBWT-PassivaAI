using BBWM.Core.Membership.Model;
using BBWM.RuntimeEditor.interfaces;

using System.Text.Json;

namespace BBWM.RuntimeEditor.services;

public class EditionDeltaCalcService : IEditionDeltaCalcService
{
    private const string innerHtmlAttr = "innerHTML";
    public const string DockAttr = "dock";
    public const string PreviousDockAttr = "previous-dock";

    public const string BbTooltipNode = "bb-tooltip";

    public RteEditionUpdate GetEditionUpdate(
        RteEdition previousEdition,
        RteEdition newEdition,
        RteDictionary dictionary,
        User submittedBy)
    {
        var result = new RteEditionUpdate
        {
            SubmittedBy = new RteEditionSumbmitUser
            {
                Name = $"{submittedBy.FirstName} {submittedBy.LastName}",
                Email = submittedBy.Email
            }
        };

        var updateList = new List<RteNodeEdits>();

        // If the edit is just created by user then we add it to the edition update as is.
        // If the edit was changed by user then we calculate and add the update edit object.
        foreach (var n in newEdition.Edits)
        {
            var p = previousEdition.Edits.FirstOrDefault(o => o.RteId == n.RteId);

            if (p is null)
            {
                updateList.Add(n);
            }
            else if (JsonSerializer.Serialize(n) != JsonSerializer.Serialize(p))
            {
                updateList.Add(CalcEditUpdate(p, n, dictionary.Items.FirstOrDefault(o => o.RteId == p.RteId)));
            }
        }

        // if the edit was removed by user then we calculate and add the recovering edit object
        previousEdition.Edits
            .Where(p => newEdition.Edits.All(n => n.RteId != p.RteId))
            .ToList()
            .ForEach(p => updateList.Add(
                CalcEditUpdate(p, null, dictionary.Items.FirstOrDefault(o => o.RteId == p.RteId))));

        result.Edits = updateList.ToArray();
        return result;
    }

    private static RteNodeEdits CalcEditUpdate(RteNodeEdits previousEdit, RteNodeEdits newEdit, RteDictionaryItem dictionaryItem)
    {
        var result = new RteNodeEdits { RteId = previousEdit.RteId };
        var updateList = new List<RteEdit>();

        if (newEdit is not null)
        {
            foreach (var n in newEdit.Edits)
            {
                var p = previousEdit.Edits.FirstOrDefault(o => o.Attr == n.Attr);

                if (p is null)
                {
                    updateList.Add(n);
                }
                else if (n.Value != p.Value)
                {
                    var editUpdate = CalcAttrEditUpdate(p, n, dictionaryItem);
                    if (editUpdate is not null)
                        updateList.Add(editUpdate);
                }
            }
        }

        previousEdit.Edits
            .Where(p => newEdit is null || newEdit.Edits.All(n => n.Attr != p.Attr))
            .ToList()
            .ForEach(p =>
            {
                var editUpdate = CalcAttrEditUpdate(p, null, dictionaryItem);
                if (editUpdate is not null)
                    updateList.Add(editUpdate);
            });

        // Adds info about the tooltip's position that was before this edition.
        // This info helps to find the previously added tooltip in markup.
        if (updateList.Any(o => RteEditAttrInfo.Parse(o.Attr).Node == BbTooltipNode))
        {
            var value = previousEdit.Edits.FirstOrDefault(o =>
            {
                var a = RteEditAttrInfo.Parse(o.Attr);
                return a.Node == BbTooltipNode && a.Attr == DockAttr;
            })?.Value;

            if (value is not null)
            {
                updateList.Add(new RteEdit
                {
                    Attr = new RteEditAttrInfo
                    {
                        IsNewNode = true,
                        Node = BbTooltipNode,
                        Attr = PreviousDockAttr
                    }.ToString(),
                    Value = value
                });
            }
        }

        result.Edits = updateList.ToArray();
        return result;
    }

    private static RteEdit CalcAttrEditUpdate(RteEdit previousAttrEdit, RteEdit newAttrEdit, RteDictionaryItem dictionaryItem)
    {
        var result = new RteEdit { Attr = previousAttrEdit.Attr };

        if (newAttrEdit is not null)
        {
            result.Value = newAttrEdit.Value;
        }
        else
        {
            var attrInfo = RteEditAttrInfo.Parse(previousAttrEdit.Attr);
            if (attrInfo.IsNewNode)
            {
                // If attr name is null it means the attr describes the whole new node then we set a command to remove the node.
                // Else it's the new node's attr and we just set it to null.
                result.Value = attrInfo.Attr is null ? DockNodeAction.Delete.ToString() : null;
            }
            else
            {
                result.Value = previousAttrEdit.Attr == innerHtmlAttr ?
                    dictionaryItem.Phrase :
                    dictionaryItem.Attrs.First(o => o.Attr == previousAttrEdit.Attr).Value;
            }
        }

        return result;
    }
}