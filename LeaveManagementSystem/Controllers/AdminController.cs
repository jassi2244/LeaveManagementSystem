using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILeaveAllocationService _allocationService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(UserManager<ApplicationUser> userManager, ILeaveAllocationService allocationService, ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _allocationService = allocationService;
        _logger = logger;
    }

    private async Task<List<ApplicationUser>> GetActiveManagersAsync()
    {
        var managers = await _userManager.GetUsersInRoleAsync("Manager");
        return managers
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .ToList();
    }

    public async Task<IActionResult> Employees(string? search)
    {
        try
        {
            var users = _userManager.Users
                .Include(x => x.Department)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(x => x.FullName.Contains(search) || x.Email!.Contains(search));
            return View(await users.OrderBy(x => x.FullName).ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Employees failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    public async Task<IActionResult> CreateEmployee()
    {
        ViewBag.Managers = await GetActiveManagersAsync();
        return View(new RegisterVM());
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee(RegisterVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await GetActiveManagersAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                ManagerId = model.Role == "Employee" ? model.ManagerId : null,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                ViewBag.Managers = await GetActiveManagersAsync();
                return View(model);
            }
            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateEmployee failed");
            TempData["Error"] = "Unable to create employee.";
            ViewBag.Managers = await GetActiveManagersAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> EditEmployee(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            ViewBag.Managers = await GetActiveManagersAsync();
            return View(new RegisterVM
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                ManagerId = user.ManagerId,
                DateOfJoining = user.DateOfJoining
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditEmployee GET failed");
            return RedirectToAction(nameof(Employees));
        }
    }

    [HttpPost]
    public async Task<IActionResult> EditEmployee(string id, RegisterVM model)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.ManagerId = isEmployee ? model.ManagerId : null;
            user.DateOfJoining = model.DateOfJoining;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EditEmployee POST failed");
            TempData["Error"] = "Unable to update employee.";
            ViewBag.Managers = await GetActiveManagersAsync();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEmployee(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
                TempData["Info"] = "Employee deactivated successfully.";
            }
            else
            {
                TempData["Warning"] = "Employee not found.";
            }
            return RedirectToAction(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEmployee failed");
            TempData["Error"] = "Unable to deactivate employee.";
            return RedirectToAction(nameof(Employees));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ReactivateEmployee(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "Employee reactivated successfully.";
            }
            else
            {
                TempData["Warning"] = "Employee not found.";
            }
            return RedirectToAction(nameof(Employees));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReactivateEmployee failed");
            TempData["Error"] = "Unable to reactivate employee.";
            return RedirectToAction(nameof(Employees));
        }
    }

    public async Task<IActionResult> EmployeeDetails(string id)
    {
        try
        {
            var user = await _userManager.Users
                .Include(x => x.Department)
                .Include(x => x.Manager)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmployeeDetails failed");
            return RedirectToAction(nameof(Employees));
        }
    }

    [HttpGet]
    public IActionResult Allocations() => View(new LeaveAllocationVM { Year = DateTime.UtcNow.Year });

    [HttpPost]
    public async Task<IActionResult> Allocations(LeaveAllocationVM model)
    {
        try
        {
            var result = await _allocationService.AllocateAsync(model, User.Identity?.Name ?? "admin");
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Allocations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Allocation save failed");
            TempData["Error"] = "Unable to save allocation.";
            return View(model);
        }
    }
}
