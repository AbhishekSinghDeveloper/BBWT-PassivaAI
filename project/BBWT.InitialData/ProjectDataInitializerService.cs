using BBWM.Core.AppEnvironment;
using BBWM.Core.Data;
using Microsoft.Extensions.Logging;

namespace BBWT.InitialData;

public partial class ProjectDataInitializerService : IProjectDataInitializerService
{
    private readonly IDbContext _context;
    private readonly IAppEnvironmentService _appEnvironmentService;
    private readonly ILogger<DatabaseInitializerService> _logger;

    public ProjectDataInitializerService(
        IDbContext context,
        IAppEnvironmentService appEnvironmentService,
        ILogger<DatabaseInitializerService> logger)
    {
        _context = context;
        _appEnvironmentService = appEnvironmentService;
        _logger = logger;
    }

    public void EnsureInitialData(bool includingOnceSeededData)
    {
        // Add project-specific data initialization here.
        // Initialization is completed with saving (context.SaveChanges())

        if (includingOnceSeededData)
        {
            // For only onces seeded data
        }
        else
        {
            // For ensuring data existance on each app start up. Also for data sync.
        }

        // Use this check to handle data seeding for "live type" environments (e.g. production/uat) 
        //if (_appEnvironmentService.IsLiveTypeEnvironment())
        //{
        //   ...
        //}
    }
}
