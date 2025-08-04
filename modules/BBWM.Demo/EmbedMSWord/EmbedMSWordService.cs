using System.Net;

namespace BBWM.Demo.EmbedMSWord;

public interface IEmbedMSWordService
{
    Task<string> RequestPageContent(string url, CancellationToken cancellationToken);
}

public class EmbedMSWordService : IEmbedMSWordService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EmbedMSWordService(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    private static dynamic GetSubStringBetween(string content, string startStr, string endStr, int offset = 0)
    {
        var indexStart = content.IndexOf(startStr, offset, StringComparison.Ordinal) + startStr.Length;
        var indexEnd = content.IndexOf(endStr, indexStart, StringComparison.Ordinal);

        if (endStr == string.Empty)
        {
            indexEnd = content.Length;
        }

        if (indexStart == -1 || indexEnd == -1)
        {
            throw new Exception(string.Format("Can't find startStr or endStr, indexStart = {0}, indexEnd = {1}", indexStart, indexEnd));
        }

        var result = content.Substring(indexStart, indexEnd - indexStart);
        return new
        {
            result,
            indexEnd
        };
    }

    public async Task<string> RequestPageContent(string url, CancellationToken cancellationToken)
    {
        // read a real url for word document
        using var httpClient = _httpClientFactory.CreateClient();

        var initialPage = await httpClient.GetAsync(url, cancellationToken);
        var initialPageContents = await initialPage.Content.ReadAsStringAsync();
        dynamic onlineWordUrlDynamic = GetSubStringBetween(initialPageContents, "content=\"0;url=", "\"");

        var onlineWordUrl = WebUtility.HtmlDecode(onlineWordUrlDynamic.result);

        var wordDocumentPage = await httpClient.GetAsync(onlineWordUrl, cancellationToken);
        var pageContents = await wordDocumentPage.Content.ReadAsStringAsync();

        return pageContents;
    }

}
