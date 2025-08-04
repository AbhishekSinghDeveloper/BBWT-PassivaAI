using BBWM.Core.Data;

namespace BBWM.Core.Services;

public class DbServices : IDbServices
{
    private readonly IMultiTenancyService _multiTenancyService;
    private readonly IAuditWrapper _auditWrapper;

    public DbServices(IAuditWrapper auditWrapper = null, IMultiTenancyService multiTenancyService = null)
    {
        _multiTenancyService = multiTenancyService;
        _auditWrapper = auditWrapper;
    }

    public IAuditWrapper GetAuditWrapper()
    {
        return _auditWrapper;
    }

    public IMultiTenancyService GetMultiTenancyService()
    {
        return _multiTenancyService;
    }
}
