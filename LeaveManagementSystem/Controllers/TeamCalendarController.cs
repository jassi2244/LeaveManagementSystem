using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class TeamCalendarController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TeamCalendarController> _logger;

    public TeamCalendarController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<TeamCalendarController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var query = _context.LeaveRequests
                .AsNoTracking()
                .Include(x => x.RequestingEmployee)
                .Include(x => x.LeaveType)
                .Where(x => x.Status == "Approved" && !x.Cancelled && x.EndDate >= DateTime.UtcNow.Date);

            if (User.IsInRole("Manager"))
            {
                var me = await _userManager.GetUserAsync(User);
                if (me == null) return RedirectToAction("Login", "Account");
                query = query.Where(x => x.RequestingEmployee != null && x.RequestingEmployee.ManagerId == me.Id);
            }

            var items = await query
                .OrderBy(x => x.StartDate)
                .Take(500)
                .ToListAsync();

            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Team calendar load failed.");
            TempData["Error"] = "Unable to load team calendar.";
            return View(new List<LeaveRequest>());
        }
    }
}
