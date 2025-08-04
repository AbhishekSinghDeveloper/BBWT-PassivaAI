using HtmlAgilityPack;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeEditor.Services;

public class EmbedLinksService : IEmbedService
{
    private readonly LinksEmbedSettings embedSettings;
    private string contentStr;

    public readonly RteIdSet idSet;
    public readonly Lookup<string, RteNodeLocation> nodeLocations;

    public EmbedLinksService(IEnumerable<RteNodeLocation> nodeLocations,
        string rteIdCounter, LinksEmbedSettings embedSettings)
    {
        idSet = new RteIdSet(nodeLocations.Select(o => o.RteId).ToHashSet(), rteIdCounter);
        this.nodeLocations = (Lookup<string, RteNodeLocation>)nodeLocations.ToLookup(o => o.RteId, o => o);
        this.embedSettings = embedSettings;
    }

    public ProcessedTemplate Embed(string content)
    {
        this.contentStr = content;

        var result = new ProcessedTemplate
        {
            OriginalContent = content,
            ResultContent = content
        };

        try
        {
            var nodes = HtmlProcessor.GetDocumentNodes(content);

            var amendments = new List<HtmlAmendment>();

            #region find nodes for new IDs
            foreach (var d in nodes)
            {
                var ins = GetNodeAmendment(d, GetNodeChangeAction(d));
                if (ins != null)
                {
                    amendments.Add(ins);
                }
            }
            #endregion

            result.ResultContent = HtmlProcessor.Amend(content, amendments);
            result.AmendmentsCount = amendments.Count;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private HtmlAmendment GetNodeAmendment(HtmlNode d, NodeChangeAction action) =>
        action switch
        {
            NodeChangeAction.Embed =>
                HtmlProcessor.GetUpdateAttrAmendment(d, new AttrValue(Constants.RteAttrName, idSet.GenerateNewId())),
            NodeChangeAction.Clean =>
                HtmlProcessor.GetRemoveAttrAmendment(contentStr, d, Constants.RteAttrName),
            _ => null
        };

    private enum NodeChangeAction { None = 0, Embed = 1, Clean = 2 }

    private NodeChangeAction GetNodeChangeAction(HtmlNode node)
    {
        var nodeName = node.Name.ToLower();

        if (node.NodeType != HtmlNodeType.Element)
            return NodeChangeAction.None;

        var nodeRteId = node.AttrValueTrim(Constants.RteAttrName);

        // RTE ID tag shouldn't be set yet, or we clean it up if it's a skipped node
        if (!string.IsNullOrEmpty(nodeRteId))
        {
            if (embedSettings.SkippedNodes.Contains(nodeName) && embedSettings.CleanupSkippedNodes)
                return NodeChangeAction.Clean;
            else
                return CheckAndSolveIdDuplication(nodeRteId, nodeLocations);
        }
        else
        {
            if (embedSettings.SkippedNodes.Contains(nodeName))
                return NodeChangeAction.None;

            // Node must be allowed for processing
            if (!embedSettings.AllowedNodes.Contains(nodeName) && !embedSettings.AllowedNodes.Contains("*"))
                return NodeChangeAction.None;

            // If empty nodes skipped 
            if (embedSettings.SkipEmptyNodes && embedSettings.SkippedEmptyNodes.Contains(nodeName))
            {
                var trimmed = node.InnerHtml.Trim(new char[] { ' ', '\r', '\n', '\t' });
                if (string.IsNullOrEmpty(trimmed))
                    return NodeChangeAction.None;
            }

            // If parent node is skipped if has disallowed childs (on +1 nesting level for now)
            if (embedSettings.SkipParentNodes)
            {
                if (!embedSettings.AllowedParentNodes.Contains(nodeName)
                     && node.ChildNodes.Any(o => o.NodeType == HtmlNodeType.Element
                            && !embedSettings.AllowedChildNodes.Contains(o.Name.ToLower()))
                    )
                    return NodeChangeAction.None;
            }

            //if node must contain attribute (example - if <input> has placeholder='...' then it worth to be anchored)
            if (embedSettings.SkipAttributeRequiredNodes
                && embedSettings.AttributeRequiredNodes.Any(o => o.Node == nodeName))
            {
                var val = embedSettings.AttributeRequiredNodes.First(o => o.Node == nodeName).Attr;
                if (!node.Attributes.Contains(val))
                    return NodeChangeAction.None;
            }
        }

        return NodeChangeAction.Embed;
    }

    // Defines a strategy of solving duplicated RTE IDs.
    // ***
    // This implementation simply renews the ID if it has any duplications throughout the repository.
    // ***
    // Future improvement can be: to take into account a node location in repository and markup structure
    // and decide whether a node is original or a new duplication (e.g. added by copy paste). But that needs
    // to put node's location info into the dictionary file so to be used as the source of changes history.
    private NodeChangeAction CheckAndSolveIdDuplication(string nodeRteId, Lookup<string, RteNodeLocation> locations)
    {
        return locations[nodeRteId].Count() > 1 ? NodeChangeAction.Embed : NodeChangeAction.None;
    }
}