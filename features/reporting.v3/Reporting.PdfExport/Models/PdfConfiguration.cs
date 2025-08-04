namespace BBF.Reporting.PdfExport.Models;

public class PdfConfiguration
{
    public string HtmlContent { get; set; } = null!;
    public string? CssRules { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? Margin { get; set; }
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public string? HeaderCssRules { get; set; }
    public string? FooterCssRules { get; set; }
}