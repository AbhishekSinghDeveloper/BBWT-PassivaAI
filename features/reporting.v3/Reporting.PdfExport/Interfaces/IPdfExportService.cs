using BBF.Reporting.PdfExport.Models;

namespace BBF.Reporting.PdfExport.Interfaces;

public interface IPdfExportService
{
    Task<byte[]> HtmlToPdf(PdfConfiguration configuration, CancellationToken ct = default);
}