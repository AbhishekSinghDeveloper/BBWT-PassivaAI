using HtmlAgilityPack;
using RuntimeEditor.Classes;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeEditor.Services;

public static class HtmlProcessor
{
    public static string Amend(string content, List<HtmlAmendment> amends)
    {
        var insertIncrement = 0;
        var output = content;

        foreach (var a in amends.OrderBy(o => o.Position))
        {
            if (a.ReplacedLength > 0)
                output = output.Remove(a.Position + insertIncrement, a.ReplacedLength);
            if (a.Text != null)
            {
                output = output.Insert(a.Position + insertIncrement, a.Text);
            }
            insertIncrement += (a.Text?.Length ?? 0) - a.ReplacedLength;
        }

        return output;
    }

    public static string AttrValueTrim(this HtmlNode node, string attr) =>
        (node.Attributes[attr]?.Value ?? "").Trim();

    public static HtmlAmendment GetUpdateAttrAmendment(HtmlNode node, AttrValue attrValue)
    {
        var amendment = new HtmlAmendment { Text = attrValue.Value };

        var nodeAttr = node.Attributes[attrValue.Attr];

        if (nodeAttr != null)
        {
            if (nodeAttr.ValueStartIndex > 0)
            {
                amendment.Position = nodeAttr.ValueStartIndex;
                amendment.ReplacedLength = nodeAttr.ValueLength;
            }
            else
            {
                // This case may happen when the RTE attribute doesn't contain any value and is defined in invalid
                // format, e.g. <p rte></p> or <p rte=></p>

                var hasEqualsSign = false;
                var attrEndsPosition = nodeAttr.StreamPosition + nodeAttr.Name.Length;
                var i = attrEndsPosition;

                for (; i < node.OwnerDocument.Text.Length; i++)
                {
                    if (node.OwnerDocument.Text[i] != ' ')
                    {
                        hasEqualsSign = node.OwnerDocument.Text[i] == '=';
                        break;
                    }
                }

                amendment.Text = $"\"{attrValue.Value}\"";

                if (hasEqualsSign)
                {
                    amendment.Position = i + 1;
                }
                else
                {
                    amendment.Position = attrEndsPosition;
                    amendment.Text = "=" + amendment.Text;
                }
            }
        }
        else
        {
            amendment.Text = $" {attrValue.Attr}=\"{attrValue.Value}\"";
            amendment.ReplacedLength = 0;

            amendment.Position = GetNodeOpenTagEndStreamPosition(node) - 1;
            if (node.OwnerDocument.Text[amendment.Position - 1] == '/')
                amendment.Position--;
        }

        return amendment;
    }

    public static HtmlAmendment GetRemoveAttrAmendment(string content, HtmlNode d, string attrName)
    {
        HtmlAmendment result = null;

        //------ Clean up action --------
        var rteAttr = d.Attributes.FirstOrDefault(o => o.Name == attrName);

        if (rteAttr != null)
        {
            var startPos = rteAttr.StreamPosition;
            var endPos = rteAttr.ValueStartIndex + rteAttr.ValueLength + 1 /*closing quote*/;

            //remove leading whitespace if possible
            if (endPos < content.Length && content[startPos - 1] == ' ' &&
                new List<char> { ' ', '/', '>' }.Contains(content[endPos]))
            {
                startPos--;
            }

            result = new HtmlAmendment
            {
                Position = startPos,
                ReplacedLength = endPos - startPos
            };
        }

        return result;
    }

    public static HtmlAmendment GetInnerHtmlAmendment(HtmlNode node, string value)
    {
        if (node.InnerStartIndex == 0)
        {
            // We may also collect errors stats here.
            return null;
        }

        return new HtmlAmendment
        {
            Text = value,
            Position = node.InnerStartIndex,
            ReplacedLength = node.InnerHtml.Length
        };
    }

    public static int IndexOfNthSubstring(string source, string substring, int startIndex, int nth = 1)
    {
        int offset = startIndex - 1;
        for (int i = 1; i <= nth; i++)
        {
            offset = source.IndexOf(substring, offset + 1);
            if (offset == -1) break;
        }
        return offset;
    }

    public static int GetNodeOpenTagEndStreamPosition(HtmlNode node)
    {
        if (node.InnerStartIndex > 0)
            return node.InnerStartIndex;

        var attrsValuesCloseBrackets = node.Attributes.Sum(o => o.Value.ToCharArray().Count(p => p == '>'));
        return IndexOfNthSubstring(node.OwnerDocument.Text, ">", node.StreamPosition + 1, attrsValuesCloseBrackets + 1) + 1;
    }

    public static int GetNodeEndStreamPosition(HtmlNode node)
    {
        if (node.NextSibling != null)
        {
            return node.NextSibling.StreamPosition;
        }

        // The logic of code in the "else" section doesn't work for some one-tag nodes like <circle />
        // because node.OwnerDocument value doesn't correspond to node's HTML. Therefore we treat
        // the node as a single-tag one and use a method that calculates a position for an opening tag.
        if (node.InnerStartIndex == 0)
        {
            return GetNodeOpenTagEndStreamPosition(node);
        }
        else
        {
            var nodeCloseBrackets = node.OuterHtml.ToCharArray().Count(p => p == '>');
            return IndexOfNthSubstring(node.OwnerDocument.Text, ">", node.StreamPosition, nodeCloseBrackets) + 1;
        }
    }

    public static int GetNodeLength(HtmlNode node) => GetNodeEndStreamPosition(node) - node.StreamPosition;

    public static IEnumerable<HtmlNode> GetDocumentNodes(string content)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        return doc.DocumentNode.Descendants();
    }

    public static IEnumerable<HtmlNode> GetDocumentNodes(string content, string attrName) =>
        GetDocumentNodes(content).Where(o => o.Attributes.Any(p => p.Name == attrName));
}