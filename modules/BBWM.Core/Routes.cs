using BBWM.Core.Web;

namespace BBWM.Core;

public static class CoreRoutes
{
    public static readonly Route Home = new("/app", "Welcome Page");
    public static readonly Route FormioPDFGenerator = new("/app/formio", "Formio PDF Generator");
}
