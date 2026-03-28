using AutoMapper;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class LeaveTypeController : AppControllerBase
{
    private readonly ILeaveTypeRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<LeaveTypeController> _logger;
    public LeaveTypeController(ILeaveTypeRepository repo, IMapper mapper, ILogger<LeaveTypeController> logger)
    {
        _repo = repo; _mapper = mapper; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return await SafeExecuteAsync(
            async () => View((await _repo.GetAllAsync()).Select(_mapper.Map<LeaveTypeVM>)),
            _logger,
            "LeaveType index failed");
    }
    public IActionResult Create() => View(new LeaveTypeVM());
    [HttpPost]
    public async Task<IActionResult> Create(LeaveTypeVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.AddAsync(_mapper.Map<LeaveType>(vm));
                TempData["Success"] = "Leave type created successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "LeaveType create failed",
            () =>
            {
                TempData["Error"] = "Unable to create leave type.";
                return View(vm);
            });
    }
    public async Task<IActionResult> Edit(int id)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                var e = await _repo.GetByIdAsync(id);
                if (e == null) return NotFound();
                return View(_mapper.Map<LeaveTypeVM>(e));
            },
            _logger,
            "LeaveType edit get failed",
            () => RedirectToAction(nameof(Index)));
    }
    [HttpPost]
    public async Task<IActionResult> Edit(LeaveTypeVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.UpdateAsync(_mapper.Map<LeaveType>(vm));
                TempData["Success"] = "Leave type updated successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "LeaveType edit post failed",
            () =>
            {
                TempData["Error"] = "Unable to update leave type.";
                return View(vm);
            });
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (await _repo.IsInUseAsync(id))
                {
                    TempData["Warning"] = "Leave type cannot be deleted because it is already used in allocations/requests.";
                    return RedirectToAction(nameof(Index));
                }

                await _repo.DeleteAsync(id);
                TempData["Info"] = "Leave type deactivated successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "LeaveType delete failed",
            () =>
            {
                TempData["Error"] = "Unable to delete leave type.";
                return RedirectToAction(nameof(Index));
            });
    }
}
