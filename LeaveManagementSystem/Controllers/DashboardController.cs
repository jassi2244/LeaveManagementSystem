using LeaveManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (User.IsInRole("Admin")) return RedirectToAction(nameof(Admin));
            if (User.IsInRole("Manager")) return RedirectToAction(nameof(Manager));
            return RedirectToAction(nameof(Employee));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard index failed");
            return RedirectToAction("Login", "Account");
        }
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        try
        {
            return View(await _dashboardService.GetAdminDashboardAsync(DateTime.UtcNow.Year));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin dashboard failed");
            ViewBag.DashboardLoadError = "Admin dashboard data could not be loaded. Verify required stored procedures exist.";
            return View(new LeaveManagementSystem.ViewModels.DashboardAdminVM());
        }
    }

    [Authorize(Roles = "Admin")]
    public Task<IActionResult> AdminDashboard() => Admin();

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> AdminChartData(int? year)
    {
        try
        {
            var selectedYear = year ?? DateTime.UtcNow.Year;
            return Json(await _dashboardService.GetAdminChartDataAsync(selectedYear));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin chart data failed");
            return StatusCode(500, new { message = "Unable to load chart data." });
        }
    }

    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Manager()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            return View(await _dashboardService.GetManagerDashboardAsync(user.Id, DateTime.UtcNow.Year));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manager dashboard failed");
            ViewBag.DashboardLoadError = "Manager dashboard data could not be loaded. Verify required stored procedures exist.";
            return View(new LeaveManagementSystem.ViewModels.DashboardManagerVM());
        }
    }

    [Authorize(Roles = "Manager")]
    public Task<IActionResult> ManagerDashboard() => Manager();

    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Employee()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            return View(await _dashboardService.GetEmployeeDashboardAsync(user.Id, DateTime.UtcNow.Year));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Employee dashboard failed");
            ViewBag.DashboardLoadError = "Employee dashboard data could not be loaded.";
            return View(new LeaveManagementSystem.ViewModels.DashboardEmployeeVM());
        }
    }

    [Authorize(Roles = "Employee")]
    public Task<IActionResult> EmployeeDashboard() => Employee();
}
