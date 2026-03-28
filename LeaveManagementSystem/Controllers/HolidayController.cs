using AutoMapper;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class HolidayController : AppControllerBase
{
    private readonly IHolidayRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<HolidayController> _logger;
    public HolidayController(IHolidayRepository repo, IMapper mapper, ILogger<HolidayController> logger)
    {
        _repo = repo; _mapper = mapper; _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return await SafeExecuteAsync(
            async () => View((await _repo.GetAllAsync()).Select(_mapper.Map<HolidayVM>)),
            _logger,
            "Holiday index failed");
    }
    public IActionResult Create() => View(new HolidayVM { HolidayDate = DateTime.Today });
    [HttpPost]
    public async Task<IActionResult> Create(HolidayVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.AddAsync(_mapper.Map<Holiday>(vm));
                TempData["Success"] = "Holiday created successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Holiday create failed",
            () =>
            {
                TempData["Error"] = "Unable to create holiday.";
                return View(vm);
            });
    }

    public async Task<IActionResult> Edit(int id)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                var holiday = await _repo.GetByIdAsync(id);
                if (holiday == null) return NotFound();
                return View(_mapper.Map<HolidayVM>(holiday));
            },
            _logger,
            "Holiday edit get failed",
            () => RedirectToAction(nameof(Index)));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(HolidayVM vm)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                if (!ModelState.IsValid) return View(vm);
                await _repo.UpdateAsync(_mapper.Map<Holiday>(vm));
                TempData["Success"] = "Holiday updated successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Holiday edit post failed",
            () =>
            {
                TempData["Error"] = "Unable to update holiday.";
                return View(vm);
            });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return await SafeExecuteAsync(
            async () =>
            {
                await _repo.DeleteAsync(id);
                TempData["Info"] = "Holiday deleted successfully.";
                return RedirectToAction(nameof(Index));
            },
            _logger,
            "Holiday delete failed",
            () =>
            {
                TempData["Error"] = "Unable to delete holiday.";
                return RedirectToAction(nameof(Index));
            });
    }
}
