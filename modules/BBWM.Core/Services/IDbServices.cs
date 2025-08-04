using BBWM.Core.Data;

namespace BBWM.Core.Services;

public interface IDbServices
{
    IAuditWrapper GetAuditWrapper();

    IMultiTenancyService GetMultiTenancyService();
}
