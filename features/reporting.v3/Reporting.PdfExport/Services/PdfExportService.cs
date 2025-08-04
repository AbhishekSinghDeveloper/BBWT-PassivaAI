using System.Runtime.InteropServices;
using BBF.Reporting.PdfExport.Interfaces;
using BBF.Reporting.PdfExport.Models;
using jsreport.Local;
using jsreport.Types;

namespace BBF.Reporting.PdfExport.Services;

public class PdfExportService : IPdfExportService
{
    public async Task<byte[]> HtmlToPdf(PdfConfiguration configuration, CancellationToken ct = default)
    {
        // TODO: HIGH. We've got issue in deployment pipeline - we manually remove jsreport.binary.linux.dll
        // for windows env, and remove jsreport.binary.dll for linux evn. So we wanted to reduce the huge build
        // size for gitlab capacity. But the problem that for the both linux and windows based apps
        // this method requires the bot libraries preloaded before the call in this method.
        // Even that here is a fork in the IF condition, both libraries are still loaded?
        // If so, I see possible solution - we can do a conditional compilation of this code block
        // depending on windows/linux env value ? Like
        // #if ENV_LINUX  jsreport.Binary.JsReportBinary.Linux.GetBinary()....
        // #else  jsreport.Binary.JsReportBinary.GetBinary()... #endif          ???

        var reportingService = new LocalReporting()
            .UseBinary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? jsreport.Binary.JsReportBinary.GetBinary()
                : jsreport.Binary.Linux.JsReportBinary.GetBinary())
            .RunInDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "jsreport"))
            .AsUtility()
            .Create();

        var options = new RenderOptions
        {
            Timeout = 300000
        };

        var template = new Template
        {
            Engine = Engine.None,
            Recipe = Recipe.ChromePdf,
            Content = GetContent(configuration),
            Chrome = GetChrome(configuration)
        };

        var report = await reportingService.RenderAsync(new RenderRequest
        {
            Template = template,
            Options = options
        }, ct);

        using var stream = new MemoryStream();
        await report.Content.CopyToAsync(stream, ct);

        // Send the PDF file to browser
        return stream.ToArray();
    }

    private static string GetContent(PdfConfiguration configuration)
    {
        var content = configuration.HtmlContent;

        if (configuration.CssRules != null)
        {
            content += $"<style>{configuration.CssRules}</style>";
        }

        return content;
    }

    private static Chrome GetChrome(PdfConfiguration configuration)
    {
        var chrome = new Chrome
        {
            Width = configuration.Width,
            Height = configuration.Height,
            HeaderTemplate = configuration.HeaderTemplate ?? "",
            FooterTemplate = configuration.FooterTemplate ?? "",
            DisplayHeaderFooter = configuration.HeaderTemplate != null || configuration.FooterTemplate != null
        };

        if (configuration.HeaderTemplate != null)
        {
            chrome.HeaderTemplate += $"<style>{configuration.HeaderCssRules}</style>";
        }

        if (configuration.FooterTemplate != null)
        {
            chrome.FooterTemplate += $"<style>{configuration.FooterCssRules}</style>";
        }

        if (configuration.Margin == null) return chrome;

        var margins = new List<string>(configuration.Margin.Split(" "));
        chrome.MarginTop = margins.Count > 0 ? margins[0] : "0";
        chrome.MarginRight = margins.Count > 1 ? margins[1] : chrome.MarginTop;
        chrome.MarginBottom = margins.Count > 2 ? margins[2] : chrome.MarginTop;
        chrome.MarginLeft = margins.Count > 3 ? margins[3] : chrome.MarginRight;

        return chrome;
    }
}