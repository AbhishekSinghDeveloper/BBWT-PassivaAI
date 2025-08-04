using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using ControllerBase = BBWM.Core.Web.ControllerBase;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace BBWM.AppConfiguration;

[Produces("application/json")]
[Route("api/app-settings")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AppConfigurationController : ControllerBase
{
    private readonly IAppConfigurationService _appSettingsService;
    private readonly bool _isParametersStoreEnabled;

    public AppConfigurationController(
        IAppConfigurationService appSettingsService,
        IConfiguration configuration)
    {
        _appSettingsService = appSettingsService;

        var configurationProviderName = configuration.GetSection("StorageSettings")?.GetValue<string>("ProviderName");
        _isParametersStoreEnabled = configurationProviderName is not null && (configurationProviderName == "AWS" || configurationProviderName == "Azure");
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default) =>
        Ok(await _appSettingsService.GetAll(cancellationToken));

    [HttpGet]
    [Route("{name}")]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken = default) =>
        Ok(await _appSettingsService.GetByName(name, cancellationToken));

    [HttpGet]
    [Route("is-enabled")]
    public IActionResult IsEnabled() => Ok(_isParametersStoreEnabled);

    [HttpPost]
    public Task<IActionResult> Save([FromBody] Parameter dto, CancellationToken cancellationToken = default)
        => NoContent(() => _appSettingsService.Put(dto, cancellationToken));

    [HttpDelete]
    [Route("{name}")]
    public Task<IActionResult> Delete(string name, CancellationToken cancellationToken = default)
        => NoContent(() => _appSettingsService.Delete(name, cancellationToken));
}
