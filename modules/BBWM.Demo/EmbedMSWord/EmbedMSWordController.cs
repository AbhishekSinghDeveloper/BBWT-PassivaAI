using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Text;

namespace BBWM.Demo.EmbedMSWord;

[Route("api/demo/embed-msword")]
public class EmbdedMSWordController : Controller
{
    public IEmbedMSWordService service;
    public ILogger<EmbdedMSWordController> logger;

    public EmbdedMSWordController(IEmbedMSWordService service, ILogger<EmbdedMSWordController> logger)
    {
        this.service = service;
        this.logger = logger;
    }

    [HttpGet, Route("request-page")]
    public async Task<ContentResult> RequestPage(string url, CancellationToken cancellationToken = default)
    {
        var htmlCode = await service.RequestPageContent(url, cancellationToken);
        return Content(htmlCode, "text/html", Encoding.UTF8);
    }

}
