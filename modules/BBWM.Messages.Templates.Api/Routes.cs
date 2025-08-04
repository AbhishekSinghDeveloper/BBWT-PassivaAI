using BBWM.Core.Web;

namespace BBWM.Messages.Templates.Api;

public static class Routes
{
    public static Route EmailTemplates => new("/app/email-templates", "Email Templates");
    public static Route EmailTemplatesDetails => new("/app/email-templates/edit/:id", "Edit Email Template");
}
