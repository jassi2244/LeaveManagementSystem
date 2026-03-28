using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Employee")]
public class EmployeeController : Controller
{
    private readonly ILogger<EmployeeController> _logger;
    public EmployeeController(ILogger<EmployeeController> logger) => _logger = logger;

    public IActionResult Index()
    {
        try { return RedirectToAction("Employee", "Dashboard"); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Employee index failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
