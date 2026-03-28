using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

public abstract class AppControllerBase : Controller
{
    protected async Task<IActionResult> SafeExecuteAsync(
        Func<Task<IActionResult>> action,
        ILogger logger,
        string errorMessage,
        Func<IActionResult>? fallback = null)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, errorMessage);
            return fallback?.Invoke() ?? RedirectToAction("Index", "Dashboard");
        }
    }
}
