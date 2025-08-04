using HtmlAgilityPack;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;

namespace RuntimeEditor.Services;

public class CleanupLinksService : IEmbedService
{
    private readonly string removeAttr;
    public CleanupLinksService(string removeAttr)
    {
        this.removeAttr = removeAttr;
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
            var desc = GetDocumentNodes(content);

            var amendments = new List<HtmlAmendment>();

            #region find nodes for new IDs
            foreach (var d in desc)
            {
                var a = HtmlProcessor.GetRemoveAttrAmendment(content, d, removeAttr);
                if (a != null)
                {
                    amendments.Add(a);
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

    private IEnumerable<HtmlNode> GetDocumentNodes(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        return doc.DocumentNode.Descendants();
    }
}