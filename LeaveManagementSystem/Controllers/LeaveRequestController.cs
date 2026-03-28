using LeaveManagementSystem.Helpers;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Controllers;

[Authorize]
public class LeaveRequestController : Controller
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LeaveRequestController> _logger;

    public LeaveRequestController(
        ILeaveRequestRepository requestRepository,
        ILeaveTypeRepository leaveTypeRepository,
        IHolidayRepository holidayRepository,
        ILeaveRequestService leaveRequestService,
        UserManager<ApplicationUser> userManager,
        ILogger<LeaveRequestController> logger)
    {
        _requestRepository = requestRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _holidayRepository = holidayRepository;
        _leaveRequestService = leaveRequestService;
        _userManager = userManager;
        _logger = logger;
    }

    private static bool CanAccessRequest(ApplicationUser actor, LeaveRequest request, bool isAdmin, bool isManager)
    {
        if (isAdmin) return true;
        if (request.RequestingEmployeeId == actor.Id) return true;
        if (isManager && request.RequestingEmployee?.ManagerId == actor.Id) return true;
        return false;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (User.IsInRole("Admin")) return View(await _requestRepository.GetAllAsync());
            if (User.IsInRole("Manager"))
            {
                var me = await _userManager.GetUserAsync(User);
                if (me == null) return RedirectToAction("Login", "Account");
                return View(await _requestRepository.GetPendingForManagerAsync(me.Id));
            }
            return RedirectToAction(nameof(MyRequests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Leave request index failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> MyRequests()
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");
            return View(await _requestRepository.GetByEmployeeAsync(me.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "My requests failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        ViewBag.LeaveTypes = await _leaveTypeRepository.GetAllAsync();
        return View(new ApplyLeaveVM { StartDate = DateTime.Today, EndDate = DateTime.Today });
    }

    [Authorize(Roles = "Employee")]
    [HttpPost]
    public async Task<IActionResult> Apply(ApplyLeaveVM model)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");
            var result = await _leaveRequestService.ApplyLeaveAsync(model, me.Id);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                ViewBag.LeaveTypes = await _leaveTypeRepository.GetAllAsync();
                return View(model);
            }
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(MyRequests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Apply leave failed");
            TempData["Error"] = "Unable to apply leave.";
            return RedirectToAction(nameof(MyRequests));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return NotFound();
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            if (!CanAccessRequest(me, request, isAdmin, isManager)) return Forbid();
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Details failed");
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return NotFound();
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            if (!CanAccessRequest(me, request, isAdmin, isManager)) return Forbid();
            return View(new ApproveLeaveVM
            {
                LeaveRequestId = request.Id,
                EmployeeName = request.RequestingEmployee?.FullName ?? "",
                LeaveTypeName = request.LeaveType?.Name ?? "",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                NumberOfDays = request.NumberOfDays
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approve GET failed");
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    public async Task<IActionResult> Approve(ApproveLeaveVM model)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");
            var request = await _requestRepository.GetByIdAsync(model.LeaveRequestId);
            if (request == null) return NotFound();
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            if (!CanAccessRequest(me, request, isAdmin, isManager))
            {
                TempData["Error"] = "You are not authorized to approve this leave request.";
                return RedirectToAction(nameof(Index));
            }
            var result = await _leaveRequestService.ApproveLeaveAsync(model.LeaveRequestId, me.Id, model.Comments);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approve POST failed");
            TempData["Error"] = "Unable to approve request.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPost]
    public async Task<IActionResult> Reject(int id, string comments)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Json(new { success = false, message = "Unauthorized." });
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return Json(new { success = false, message = "Leave request not found." });
            var isAdmin = User.IsInRole("Admin");
            var isManager = User.IsInRole("Manager");
            if (!CanAccessRequest(me, request, isAdmin, isManager))
                return Json(new { success = false, message = "You are not authorized to reject this leave request." });
            var result = await _leaveRequestService.RejectLeaveAsync(id, me.Id, comments);
            return Json(new { success = result.Success, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reject failed");
            return Json(new { success = false, message = "Reject failed." });
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Json(new { success = false, message = "Unauthorized." });
            var result = await _leaveRequestService.CancelLeaveAsync(id, me.Id);
            return Json(new { success = result.Success, message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel failed");
            return Json(new { success = false, message = "Cancel failed." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CalculateWorkingDays(DateTime startDate, DateTime endDate)
    {
        try
        {
            var holidays = await _holidayRepository.GetBetweenAsync(startDate, endDate);
            var days = LeaveCalculationHelper.CalculateWorkingDays(startDate, endDate, holidays);
            return Json(new { workingDays = days });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CalculateWorkingDays failed");
            return Json(new { workingDays = 0 });
        }
    }
}
