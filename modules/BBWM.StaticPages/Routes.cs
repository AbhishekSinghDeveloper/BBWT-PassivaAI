using BBWM.Core.Web;

namespace BBWM.StaticPages;

public static class Routes
{
    public static Route StaticPages => new("/app/static/pages", "Static Pages");
    public static Route StaticPagesDetails => new("/app/static/pages/edit/:id", "Edit Static Page");
}
