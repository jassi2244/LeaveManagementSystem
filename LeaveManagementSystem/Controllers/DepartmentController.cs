using AutoMapper;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class DepartmentController : AppControllerBase
{
    private readonly IDepartmentRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<DepartmentController> _logger;
    public DepartmentController(IDepartmentRepository repo, IMapper mapper, ILogger<DepartmentController> logger)
    {
        _repo = repo; _mapper = mapper; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return await SafeExecuteAsync(
            async () => View((await _repo.GetAllAsync()).Select(_mapper.Map<DepartmentVM>)),
            _logger,
            "Department index failed");
    }
    public IActionResult Create() => View(new DepartmentVM());
    [HttpPost]
    public async Task<IActionResult> Create(DepartmentVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.AddAsync(_mapper.Map<Department>(vm));
                TempData["Success"] = "Department created successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Department create failed",
            () =>
            {
                TempData["Error"] = "Unable to create department.";
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
                return View(_mapper.Map<DepartmentVM>(e));
            },
            _logger,
            "Department edit get failed",
            () => RedirectToAction(nameof(Index)));
    }
    [HttpPost]
    public async Task<IActionResult> Edit(DepartmentVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.UpdateAsync(_mapper.Map<Department>(vm));
                TempData["Success"] = "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Department edit post failed",
            () =>
            {
                TempData["Error"] = "Unable to update department.";
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
                    TempData["Warning"] = "Department cannot be deleted because it is in use by active employees.";
                    return RedirectToAction(nameof(Index));
                }

                await _repo.DeleteAsync(id);
                TempData["Info"] = "Department deactivated successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Department delete failed",
            () =>
            {
                TempData["Error"] = "Unable to delete department.";
                return RedirectToAction(nameof(Index));
            });
    }
}
