namespace BBWM.DataProcessing.Services;

public interface IViewRenderService
{
    Task<string> RenderToString(string viewName, object model);
}
