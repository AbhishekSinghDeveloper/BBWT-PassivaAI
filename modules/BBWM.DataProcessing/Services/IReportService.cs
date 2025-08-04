using BBWM.DataProcessing.Classes;

namespace BBWM.DataProcessing.Services;

public interface IReportService
{
    Task<byte[]> HtmlToPdf(string htmlReport);
    Task<byte[]> PageToPdf(string url);
    Task<byte[]> FormioHtmlToPdf(FormioHTMLData reportData);
}
