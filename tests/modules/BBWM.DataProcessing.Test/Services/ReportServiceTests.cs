using BBWM.DataProcessing.Services;

using Moq;

using RichardSzalay.MockHttp;

using System.Net;
using System.Net.Mime;
using System.Text;

using Xunit;

namespace BBWM.DataProcessing.Test.Services;

public class ReportServiceTests
{
    private static string HTML => "<html><body><p><strong>Report</strong></p></body></html>";

    [Fact(Skip = "Until Linux issue is fixed")]
    public async Task Html_To_Pdf_Test()
    {
        // Arrange
        var sut = new ReportService(Mock.Of<IHttpClientFactory>());

        // Act
        var pdfBytes = await sut.HtmlToPdf(HTML);

        // Assert
        AssertPDFHeader(pdfBytes);
    }

    [Fact(Skip = "Until Linux issue is fixed")]
    public async Task Page_To_Pdf_Test()
    {
        // Arrange
        const string URL = "http://test.com/my-pdf";
        var response = new HttpResponseMessage
        {
            Content = new StringContent(HTML),
        };

        var messageHandler = new MockHttpMessageHandler();
        messageHandler.When(URL).Respond(HttpStatusCode.OK, MediaTypeNames.Text.Html, HTML);
        var client = messageHandler.ToHttpClient();

        var clientFactory = new Mock<IHttpClientFactory>();
        clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var sut = new ReportService(clientFactory.Object);

        // Act
        var pdfBytes = await sut.PageToPdf(URL);

        // Assert
        AssertPDFHeader(pdfBytes);
    }

    private void AssertPDFHeader(byte[] pdfBytes)
    {
        var headerBytes = new byte[] { 37, 80, 68, 70 };

        Assert.Equal(headerBytes, pdfBytes[..4]);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdfBytes[..5]));
    }
}
