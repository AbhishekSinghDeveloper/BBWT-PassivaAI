using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Moq;

using System.Security.Claims;

namespace BBWM.Core.Test;

public class ServicesFactory
{
    public static IHttpContextAccessor GetHttpContextAccessor(IList<Claim> claims = null)
    {
        if (claims == null)
        {
            claims = new List<Claim>();
        }

        if (claims.Any(x => x.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, "claim-name-default"));
        }

        if (claims.Any(x => x.Type == ClaimTypes.NameIdentifier))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "claim-nameidentifier-default"));
        }

        var accessor = new HttpContextAccessor()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock")),
            },
        };

        accessor.HttpContext.Request.Headers["X-Browser-Id"] = "Edge 95.0.1020.44";
        accessor.HttpContext.Request.Headers["X-Browser-Fingerprint"] = "1885987160";

        return accessor; 
    }

    public static IWebHostEnvironment GetWebHostEnvironment(bool isDevelopment, string contentRootPath = null)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(p => p.ApplicationName).Returns(isDevelopment ? "Development" : "Production");
        environment.Setup(p => p.EnvironmentName).Returns(isDevelopment ? "Development" : "Production");

        if (!string.IsNullOrEmpty(contentRootPath))
        {
            environment.Setup(p => p.ContentRootPath).Returns(contentRootPath);
        }

        return environment.Object;
    }

}
