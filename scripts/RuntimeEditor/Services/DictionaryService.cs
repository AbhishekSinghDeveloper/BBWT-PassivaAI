using HtmlAgilityPack;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeEditor.Services;

public static class DictionaryService
{
    public static List<RteDictionaryItem> Generate(List<TemplateFile> files, DictionarySettings settings)
    {
        var result = new List<RteDictionaryItem>();

        try
        {
            foreach (var file in files)
            {
                var nodes = HtmlProcessor.GetDocumentNodes(file.Content);

                foreach (var node in nodes)
                {
                    var dicItem = GetNodeDictionaryItem(node, settings);

                    if (dicItem != null)
                        result.Add(dicItem);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" >>>>>> Error generating the phrases dictionary: " + ex.Message);
        }

        return result;
    }

    private static RteDictionaryItem GetNodeDictionaryItem(HtmlNode d, DictionarySettings settings)
    {
        if (!d.Attributes.Contains(Constants.RteAttrName))
        {
            return null;
        }

        string phrase = null;

        if ((!string.IsNullOrWhiteSpace(d.InnerHtml)
                || settings.InnerHtmlNodes.Contains(d.Name.ToLowerInvariant())
            )
            && d.ChildNodes.All(
                o => o.NodeType != HtmlNodeType.Element
                || settings.AllowedChildNodes.Contains(o.Name.ToLowerInvariant())
            )
            && isValueAllowedToEdit(d.InnerHtml))
        {
            phrase = d.InnerHtml;
        }

        var attrs = settings.AllowedNodeAttrs
                .Where(o => o.Node == d.Name.ToLowerInvariant())
                .Select(o => new RteDictionaryItemAttr
                {
                    Attr = o.Attr,
                    Value = d.Attributes[o.Attr]?.Value
                })
                .Where(o => isValueAllowedToEdit(o.Value));

        // Node is not of interest, not editable. Skip it from adding to the dicionary.
        if (phrase == null && !attrs.Any())
        {
            return null;
        }

        var result = new RteDictionaryItem
        {
            RteId = d.AttrValueTrim(Constants.RteAttrName),
            Type = RteDictionaryItemType.Html,
            Phrase = phrase,
            Attrs = attrs.ToArray()
        };

        return result;
    }

    // Here we filter out attributes with special language-specific markups 
    private static bool isValueAllowedToEdit(string value)
    {
        if (value == null)
            return true;

        // Exclude Angular markup's {{ ... }} because that values contain evaluated values, not phrases
        var startPos = value.IndexOf("{{");
        var endPos = value.IndexOf("}}");
        if (startPos != -1 && endPos > startPos)
            return false;


        return true;
    }
}