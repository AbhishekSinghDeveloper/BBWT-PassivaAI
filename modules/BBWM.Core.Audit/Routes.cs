using BBWM.Core.Web;

namespace BBWM.Core.Audit;

public static class Routes
{
    public static Route DataAudit => new("/app/admin/data-audit", "Data Audit");
}
