using HtmlAgilityPack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using NetEscapades.AspNetCore.SecurityHeaders;

using System.Net.Mime;
using System.Text;

namespace BBWM.Core.Security;

public static class AppBuilderExtensions
{
    public static IApplicationBuilder UseSetContentSecurityPolicyNonce(this IApplicationBuilder app)
        => app.Use(async (httpContext, next) =>
        {
            var originalBodyResponse = httpContext.Features.Get<IHttpResponseBodyFeature>();

            using var response = new MemoryStream();
            httpContext.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(response));

            await next();

            var nonceSet = true;
            var contentType = httpContext.Response.ParseContentType();

            if (contentType?.MediaType == MediaTypeNames.Text.Html)
            {
                var htmlDoc = await BuildHtmlDocumentAsync(response);
                nonceSet = await htmlDoc.SetNonceValueAsync(httpContext, originalBodyResponse.Stream);
            }

            if (contentType?.MediaType != MediaTypeNames.Text.Html || !nonceSet)
            {
                response.Seek(0, SeekOrigin.Begin);
                await response.CopyToAsync(originalBodyResponse.Stream);
            }

            httpContext.Features.Set(originalBodyResponse);
        });

    private static async Task<HtmlDocument> BuildHtmlDocumentAsync(MemoryStream response)
    {
        var reader = new StreamReader(response);
        response.Seek(0, SeekOrigin.Begin);
        var html = await reader.ReadToEndAsync();

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        return htmlDoc;
    }

    private static async Task<bool> SetNonceValueAsync(
        this HtmlDocument htmlDoc,
        HttpContext httpContext,
        Stream originalResponse)
    {
        if (htmlDoc.ParseErrors.Any())
            return false;

        var nonce = httpContext.GetNonce();
        var nodes = htmlDoc.DocumentNode?.SelectNodes("//script");

        if (nodes is null || !nodes.Any())
            return false;

        foreach (var node in nodes)
        {
            node.SetAttributeValue("nonce", nonce);
        }

        var outputBytes = Encoding.UTF8.GetBytes(htmlDoc.DocumentNode.OuterHtml);
        // Let Asp.Net decide what's the best value for Content-Length header (should be outputBytes.Length)
        httpContext.Response.ContentLength = null;
        await originalResponse.WriteAsync(outputBytes, 0, outputBytes.Length);

        return true;
    }

    private static ContentType ParseContentType(this HttpResponse response)
    {
        if (string.IsNullOrEmpty(response.ContentType))
            return default;

        try
        {
            return new ContentType(response.ContentType);
        }
        catch
        {
            return default;
        }
    }
}
