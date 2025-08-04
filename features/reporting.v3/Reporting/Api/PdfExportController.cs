using BBF.Reporting.PdfExport.Interfaces;
using BBF.Reporting.PdfExport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Api;

[Route("api/reporting3/pdf-export")]
[Authorize]
public class PdfExportController : BBWM.Core.Web.ControllerBase
{
    private readonly IPdfExportService _pdfExportService;

    public PdfExportController(IPdfExportService pdfExportService)
        => _pdfExportService = pdfExportService;

    [HttpPost("html-to-pdf")]
    public async Task<IActionResult> HtmlToPdf([FromBody] PdfConfiguration configuration,
        CancellationToken ct = default)
        => File(await _pdfExportService.HtmlToPdf(configuration, ct), "application/pdf");
}