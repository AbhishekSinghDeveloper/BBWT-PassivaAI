using HtmlAgilityPack;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RuntimeEditor.Services;

public partial class EmbedEditsService : IEmbedService
{
    readonly RteEditionUpdate edition;
    readonly IEmbedService embedLinksService;

    private const string innerHtmlAttr = "innerHTML";

    public EmbedEditsService(IEmbedService embedLinksService, RteEditionUpdate edition)
    {
        this.edition = edition;
        this.embedLinksService = embedLinksService;
    }

    public ProcessedTemplate Embed(string content)
    {
        var result = new ProcessedTemplate
        {
            OriginalContent = content,
            ResultContent = content
        };

        try
        {
            var doc = new HtmlDocument();
            doc.Load(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var desc = doc.DocumentNode.Descendants();

            var amends = new List<HtmlAmendment>();

            var edits = edition.Edits.ToList();

            foreach (var d in desc)
            {
                if (d.Attributes.Contains(Constants.RteAttrName))
                {
                    var arrV = edits.Find(o => o.RteId == d.AttrValueTrim(Constants.RteAttrName));

                    if (arrV != null)
                    {
                        var l = GetNodeAmendment(d, arrV);
                        amends.AddRange(l);
                    }
                }
            }

            result.ResultContent = HtmlProcessor.Amend(content, amends);
            result.AmendmentsCount = amends.Count;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private List<HtmlAmendment> GetNodeAmendment(HtmlNode node, RteNodeEdits nodeEdits)
    {
        var result = new List<HtmlAmendment>();

        #region Amending a node docked to this processed node
        var dockNodeEdit = RteDockNodeEdit.Parse(nodeEdits.Edits);
        if (dockNodeEdit != null)
        {
            result.AddRange(GetDockedNodeAmendment(node, dockNodeEdit));
        }
        #endregion

        #region Amending the internal attributes of the node
        var attrEdits = nodeEdits.Edits.Where(o => !RteEditAttrInfo.Parse(o.Attr).IsNewNode).ToList();
        var attrAmendments = GetNodeAttrsAmendments(node, attrEdits);
        result.AddRange(attrAmendments);
        #endregion

        return result;
    }

    private List<HtmlAmendment> GetNodeAttrsAmendments(HtmlNode node, List<RteEdit> attrEdits) =>
        attrEdits
            .ConvertAll(o => o.Attr == innerHtmlAttr ?
                HtmlProcessor.GetInnerHtmlAmendment(node, o.Value) :
                HtmlProcessor.GetUpdateAttrAmendment(node, new AttrValue(o.Attr, o.Value)))
            .FindAll(o => o != null);

    private List<HtmlAmendment> GetDockedNodeAmendment(HtmlNode parentNode, RteDockNodeEdit edit)
    {
        switch (edit.Action)
        {
            case DockNodeAction.Create:
                return new List<HtmlAmendment> { GetDockedNodeCreateAmendment(parentNode, edit) };

            case DockNodeAction.Update:
                return GetDockedNodeUpdateAmendment(parentNode, edit);

            case DockNodeAction.Delete:
                var deleteAmendment = GetDockedNodeDeleteAmendment(parentNode, edit);
                return deleteAmendment == null ?
                    new List<HtmlAmendment> { } :
                    new List<HtmlAmendment> { deleteAmendment };
        }

        return new List<HtmlAmendment> { };
    }

    private HtmlAmendment GetDockedNodeCreateAmendment(HtmlNode parentNode, RteDockNodeEdit edit)
    {
        var attrsStr = edit.Attrs
                .Select(o => $" {o.Attr}=\"{o.Value}\"")
                .Aggregate("", (a, b) => a + b);

        var nodeHtml = $"<{edit.Node}{attrsStr}></{edit.Node}>";

        var processed = this.embedLinksService.Embed(nodeHtml);

        return new HtmlAmendment
        {
            Text = processed.ResultContent,
            Position = GetDockedNodeInsertPosition(parentNode, edit.Dock),
            ReplacedLength = 0
        };
    }

    private int GetDockedNodeInsertPosition(HtmlNode parentNode, DockTo dock) =>
        dock == DockTo.Left ? parentNode.StreamPosition : HtmlProcessor.GetNodeEndStreamPosition(parentNode);

    private List<HtmlAmendment> GetDockedNodeUpdateAmendment(HtmlNode parentNode, RteDockNodeEdit edit)
    {
        var attrEdits = edit.Attrs.ConvertAll(o => new RteEdit { Attr = o.Attr, Value = o.Value });
        var dockedNode = FindDockedNode(parentNode, edit);
        if (dockedNode == null)
            return new List<HtmlAmendment>();

        var result = new List<HtmlAmendment>();

        var attrAmendments = GetNodeAttrsAmendments(dockedNode, attrEdits);

        if (edit.Dock == edit.PreviousDock)
        {
            result.AddRange(attrAmendments);
        }
        // If position of the docked node is swapped (left dock -> right dock or opposite) then we add ammendpents to
        // cut and paste the node
        else
        {
            // Cut the docked node
            var deleteAmendment = GetDeleteNodeAmendment(dockedNode);
            result.Add(deleteAmendment);

            // Paste the docked node
            var relativeAttrAmendments = attrAmendments.ConvertAll(o => new HtmlAmendment
            {
                Position = o.Position - dockedNode.StreamPosition,
                ReplacedLength = o.ReplacedLength,
                Text = o.Text
            });

            var dockedNodeHtml = dockedNode.OwnerDocument.Text.Substring(
                    dockedNode.StreamPosition, HtmlProcessor.GetNodeLength(dockedNode));
            var amendedNodeHtml = HtmlProcessor.Amend(dockedNodeHtml, relativeAttrAmendments);

            var insertAmendment = new HtmlAmendment
            {
                Position = GetDockedNodeInsertPosition(parentNode, edit.Dock),
                Text = amendedNodeHtml
            };
            result.Add(insertAmendment);
        }

        return result;
    }

    private HtmlNode FindDockedNode(HtmlNode parentNode, RteDockNodeEdit edit)
    {
        var dockedNode = edit.PreviousDock == DockTo.Left ? parentNode.PreviousSibling : parentNode.NextSibling;
        if (dockedNode == null || !dockedNode.Name.Equals(edit.Node, StringComparison.InvariantCultureIgnoreCase))
            return null;
        return dockedNode;
    }

    private HtmlAmendment GetDockedNodeDeleteAmendment(HtmlNode parentNode, RteDockNodeEdit edit)
    {
        var dockedNode = FindDockedNode(parentNode, edit);

        if (dockedNode == null)
            return null;

        return GetDeleteNodeAmendment(dockedNode);
    }

    private HtmlAmendment GetDeleteNodeAmendment(HtmlNode node) =>
        new HtmlAmendment
        {
            Position = node.StreamPosition,
            ReplacedLength = HtmlProcessor.GetNodeLength(node)
        };
}