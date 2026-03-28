using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    private readonly ILogger<ManagerController> _logger;
    public ManagerController(ILogger<ManagerController> logger) => _logger = logger;

    public IActionResult Index()
    {
        try { return RedirectToAction("Manager", "Dashboard"); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manager index failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
