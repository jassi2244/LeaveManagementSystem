using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View(new LoginVM());

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        try
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Account is temporarily locked due to failed attempts. Try again later.");
                return View(model);
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            ModelState.AddModelError("", "Login failed.");
            return View(model);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        ViewBag.Departments = new List<Models.Department>();
        ViewBag.Managers = await _userManager.GetUsersInRoleAsync("Manager");
        return View(new RegisterVM());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = await _userManager.GetUsersInRoleAsync("Manager");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                DepartmentId = model.DepartmentId,
                ManagerId = model.ManagerId,
                DateOfJoining = model.DateOfJoining,
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                ViewBag.Managers = await _userManager.GetUsersInRoleAsync("Manager");
                return View(model);
            }
            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["Success"] = "User registered.";
            return RedirectToAction(nameof(Register));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register failed");
            TempData["Error"] = "Register failed.";
            return RedirectToAction(nameof(Register));
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
