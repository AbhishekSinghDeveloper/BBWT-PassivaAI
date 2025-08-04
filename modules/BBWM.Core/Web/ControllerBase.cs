using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Web;

/// <summary>
/// Represents the class of base controller
/// </summary>
public abstract class ControllerBase : Controller
{
    protected async Task<IActionResult> NoContent(Func<Task> action)
    {
        await action();
        return NoContent();
    }

    protected IActionResult NoContent(Action action)
    {
        action();
        return NoContent();
    }
}
