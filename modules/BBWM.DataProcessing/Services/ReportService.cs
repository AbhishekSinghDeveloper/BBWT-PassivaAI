using BBWM.DataProcessing.Classes;
using jsreport.Client;
using jsreport.Local;
using jsreport.Types;
using System.Runtime.InteropServices;

namespace BBWM.DataProcessing.Services;

/// <summary>
/// Service for creating reports using node services
/// </summary>
public class ReportService : IReportService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    /// <summary>
    /// Convert html to pdf
    /// </summary>
    /// <param name="htmlReport">HTML report</param>
    public async Task<byte[]> HtmlToPdf(string htmlReport)
    {
        var rs = new LocalReporting()
           .UseBinary(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? jsreport.Binary.JsReportBinary.GetBinary() : jsreport.Binary.Linux.JsReportBinary.GetBinary())
           .RunInDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "jsreport"))
           .AsUtility()
           .Create();

        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Engine = Engine.None,
                Recipe = Recipe.ChromePdf,
                Content = htmlReport
            },
            Options = new RenderOptions
            {
                Timeout = 300000
            }
        });

        using var ms = new System.IO.MemoryStream();
        report.Content.CopyTo(ms);

        // send the PDF file to browser
        return ms.ToArray();
    }

    public async Task<byte[]> FormioHtmlToPdf(FormioHTMLData reportData)
    {
        try
        {
            return await GeneratePDFWithJsReportLocalServer(reportData);

        }
        catch (Exception)
        {
            // option to generate the PDF with JsReport Online
            return await GeneratePDFWithJsReportOnline(reportData);
        }
    }

    /// <summary>
    /// Downloading the page and converting html to pdf
    /// </summary> 
    /// <param name="url">The address of the page to load</param>
    public async Task<byte[]> PageToPdf(string url)
    {
        using var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        return await HtmlToPdf(html);
    }

    private async Task<byte[]> GeneratePDFWithJsReportOnline(FormioHTMLData reportData)
    {
        var rs = new ReportingService("https://formio-demo.jsreportonline.net/", "jcreyes9204@gmail.com", "rJvQEHcaUSfgp3@");

        var data = new
        {
            HtmlContent = reportData.HtmlContent
        };

        var report = await rs.RenderByNameAsync("formio-template", data);

        using var ms = new System.IO.MemoryStream();
        report.Content.CopyTo(ms);

        // send the PDF file to browser
        return ms.ToArray();
    }

    private async Task<byte[]> GeneratePDFWithJsReportLocalServer(FormioHTMLData reportData)
    {
        var rs = new ReportingService("http://localhost:5488");

        var data = new
        {
            HtmlContent = reportData.HtmlContent
        };

        var report = await rs.RenderByNameAsync("formio-template", data);

        using var ms = new System.IO.MemoryStream();
        report.Content.CopyTo(ms);

        // send the PDF file to browser
        return ms.ToArray();
    }

}
