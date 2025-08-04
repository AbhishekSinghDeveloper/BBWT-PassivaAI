using Microsoft.AspNetCore.Http;

namespace BBWM.Core.Web.Extensions;

public static class HttpRequestExtensions
{
    public static string GetDomainUrl(this HttpRequest request)
        => $"{request.Scheme}://{request.Host.Value}";
}
