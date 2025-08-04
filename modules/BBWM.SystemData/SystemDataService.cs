using BBWM.Core.ModuleLinker;
using BBWM.Core.Web.Middlewares;
using BBWM.SystemData.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace BBWM.SystemData;

public class SystemDataService : ISystemDataService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IProductVersionService _productVersionService;
    private readonly ILogger<SystemDataService> _logger;

    public SystemDataService(
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostingEnvironment,
        IProductVersionService productVersionService,
        ILogger<SystemDataService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _hostingEnvironment = hostingEnvironment;
        _productVersionService = productVersionService;
        _logger = logger;
    }

    public async Task<SystemSummaryDTO> GetSystemSummary()
        => new()
        {
            // Server Info
            ServerEnvironment = _hostingEnvironment.EnvironmentName,
            ServerName = Environment.MachineName,
            ServerIp = await GetIpAddress(),
            OperatingSystem = RuntimeInformation.OSDescription,
            // Client Info
            UserName = _httpContextAccessor.HttpContext.User.Identity.Name,
            ClientIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
        };

    private async Task<string> GetIpAddress()
    {
        var sourceUrl = "https://api.ipify.org";
        try
        {
            return await new HttpClient().GetStringAsync(sourceUrl);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Couldn't get IP address from {sourceUrl}", ex);
            return null;
        }
    }

    public VersionInfoDTO GetVersionInfo()
        => ParseProductVersionInfo(_productVersionService.GetVersion());

    public DebugInfoDTO GetDebugInfo()
        => new()
        {
            LinkedModules = ModuleLinker.LinkedClasses.Select(o => new KeyValuePair<string, string>(o.Key.FullName, o.Value)),
            LinkerInvokeExceptions = ModuleLinker.InvokeExceptions.Select(ExceptionToOutput),
            LinkerCommonExceptions = ModuleLinker.CommonExceptions.Select(ExceptionToOutput),
            ApiExceptions = ErrorHandlingMiddleware.HandlerExceptionsDebug.Select(ExceptionToOutput)
        };

    public IEnumerable<OutputExceptionDTO> GetApiExceptions()
        => ErrorHandlingMiddleware.HandlerExceptionsDebug.Select(ExceptionToOutput);

    /// Version content format: [template version (3.x.x)]-[pipeline ID]-[commit hash]-[Git project name]-[Git project ID]
    private static VersionInfoDTO ParseProductVersionInfo(string version)
    {
        var versionInfo = new VersionInfoDTO { FullProductVersion = version };
        var parts = version.Split('-');

        if (parts.Length > 1)
        {
            var i = 0;

            if (i < parts.Length)
                versionInfo.ProductVersion = parts[i++];

            // Special case - to take into account a version in development like "3.x.x-next"
            if (i < parts.Length && parts[i] == "next")
                versionInfo.ProductVersion += "-" + parts[i++];

            if (i < parts.Length)
                versionInfo.Pipeline = parts[i++];

            if (i < parts.Length)
                versionInfo.CommitHash = parts[i++];

            if (i < parts.Length)
                versionInfo.ProjectName = parts[i++];

            if (i < parts.Length)
                versionInfo.ProjectID = parts[i++];
        }

        return versionInfo;
    }

    private OutputExceptionDTO ExceptionToOutput(Exception ex) =>
        new()
        {
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source,
            InnerExMessage = ex.InnerException?.Message,
            InnerExStackTrace = ex.InnerException?.StackTrace,
            InnerExSource = ex.InnerException?.Source
        };
}
