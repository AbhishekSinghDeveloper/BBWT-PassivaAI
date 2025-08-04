using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Services;

using Microsoft.AspNetCore.Mvc;

using System.Text;

namespace BBWM.Demo.Controllers;

/// <summary>
/// Provides converting HTML to PDF.
/// </summary>
[Route("api/demo/converter")]
public class ConverterController : Controller
{
    private readonly IReportService _reportService;


    public ConverterController(IReportService reportService) => _reportService = reportService;


    /// <summary>
    /// Converts HTML to a PDF file.
    /// </summary>
    [HttpPost]
    [Route("html-to-pdf")]
    public async Task<FileResult> HtmlToPdf()
    {
        using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        var result = await _reportService.HtmlToPdf(body);
        return File(result, "application/pdf");
    }

    /// <summary>
    /// Test -- Converts HTML to a PDF file.
    /// </summary>
    [HttpPost]
    [Route("html-to-pdf-formio")]
    public async Task<FileResult> FormioHtmlToPdf([FromBody] FormioHTMLData data)
    {
        var result = await _reportService.FormioHtmlToPdf(data);
        return File(result, "application/pdf");
    }

    /// <summary>
    /// Download page converts it to PDF file
    /// </summary>
    /// <param name="url">Url of the page being downloaded</param>
    [HttpGet]
    [Route("url-to-pdf")]
    public async Task<FileResult> UrlToPdf(string url)
    {
        var result = await _reportService.PageToPdf(url);
        return File(result, "application/pdf");
    }
}
