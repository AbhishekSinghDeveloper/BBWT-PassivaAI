using Microsoft.AspNetCore.Mvc;

using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.Services;

namespace BBWM.FormIO.Controllers;

[Route("api/formio-pdf-converter")]
public class FomIOPDFGeneratorController : Controller
{
    private readonly IReportService _reportService;

    public FomIOPDFGeneratorController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Converts HTML to a PDF file using jsreport library
    /// </summary>
    [HttpPost]
    [Route("html-to-pdf")]
    public async Task<FileResult> FormioHtmlToPdf([FromBody] FormioHTMLData data)
    {
        var result = await _reportService.FormioHtmlToPdf(data);
        return File(result, "application/pdf");
    }
}