using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Helpers;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using LeaveManagementSystem.ViewModels;
using Microsoft.Extensions.Options;

namespace LeaveManagementSystem.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly INotificationService _notificationService;
    private readonly LeavePolicyOptions _leavePolicyOptions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveAllocationRepository leaveAllocationRepository,
        IHolidayRepository holidayRepository,
        INotificationService notificationService,
        IOptions<LeavePolicyOptions> leavePolicyOptions,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache,
        ILogger<LeaveRequestService> logger)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _leaveAllocationRepository = leaveAllocationRepository;
        _holidayRepository = holidayRepository;
        _notificationService = notificationService;
        _leavePolicyOptions = leavePolicyOptions.Value;
        _userManager = userManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ServiceResult> ApplyLeaveAsync(ApplyLeaveVM model, string userId)
    {
        if (model.StartDate.Date < DateTime.UtcNow.Date) return ServiceResult.Fail("Start date cannot be in the past.");
        if (_leavePolicyOptions.MinimumNoticeDays > 0 &&
            model.StartDate.Date < DateTime.UtcNow.Date.AddDays(_leavePolicyOptions.MinimumNoticeDays))
        {
            return ServiceResult.Fail($"Leave must be applied at least {_leavePolicyOptions.MinimumNoticeDays} day(s) in advance.");
        }
        if (model.EndDate.Date < model.StartDate.Date) return ServiceResult.Fail("End date must be after start date.");

        var holidays = await _holidayRepository.GetBetweenAsync(model.StartDate, model.EndDate);
        var workingDays = CalculateBillableDays(model, holidays);
        if (holidays.Any(h => h.HolidayDate.Date == model.StartDate.Date || h.HolidayDate.Date == model.EndDate.Date))
            return ServiceResult.Fail("You cannot apply leave on a holiday date.");
        if (workingDays <= 0)
            return ServiceResult.Fail("Selected dates are non-working days. You cannot apply leave on holidays/weekends.");

        if (await _leaveRequestRepository.HasOverlappingAsync(userId, model.StartDate, model.EndDate))
            return ServiceResult.Fail("Overlapping leave request exists.");

        var targetYear = model.StartDate.Year;
        var balances = await _leaveAllocationRepository.GetLeaveBalanceBySpAsync(userId, targetYear);
        var selectedBalance = balances.FirstOrDefault(x => x.LeaveTypeId == model.LeaveTypeId);
        if (selectedBalance == null || selectedBalance.RemainingDays < workingDays)
            return ServiceResult.Fail("Insufficient leave balance.");

        // sp_GetLeaveBalance falls back to LeaveType default days even if no allocation row exists.
        // sp_ApplyLeave requires a LeaveAllocations row, so create one lazily when missing.
        var existingAllocation = await _leaveAllocationRepository.GetAsync(userId, model.LeaveTypeId, targetYear);
        if (existingAllocation == null)
        {
            var allocationResult = await _leaveAllocationRepository.AllocateBySpAsync(
                userId,
                model.LeaveTypeId,
                selectedBalance.TotalDays,
                targetYear);

            if (allocationResult.Result != 1)
                return ServiceResult.Fail(allocationResult.Message);
        }

        var result = await _leaveRequestRepository.ApplyBySpAsync(
            userId,
            model.LeaveTypeId,
            model.StartDate.Date,
            model.EndDate.Date,
            workingDays,
            model.Reason);

        if (result.Result == 1)
        {
            _logger.LogInformation("Leave applied. UserId: {UserId}, LeaveTypeId: {LeaveTypeId}, Start: {StartDate}, End: {EndDate}, Days: {Days}, HalfDay: {IsHalfDay}",
                userId, model.LeaveTypeId, model.StartDate, model.EndDate, workingDays, model.IsHalfDay);
            CompactCache();
            await _notificationService.SendAsync(userId, "Leave request submitted", result.Message);
            return ServiceResult.Ok(result.Message);
        }

        return ServiceResult.Fail(result.Message);
    }

    public async Task<ServiceResult> ApproveLeaveAsync(int leaveRequestId, string approvedById, string? comments)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (request == null) return ServiceResult.Fail("Leave request not found.");

        var approver = await _userManager.FindByIdAsync(approvedById);
        var isAdmin = approver != null && await _userManager.IsInRoleAsync(approver, "Admin");
        var isManager = approver != null && await _userManager.IsInRoleAsync(approver, "Manager");

        if (isManager && !isAdmin)
        {
            request.Status = "PendingAdminApproval";
            request.ReviewComments = comments;
            request.ApprovedById = approvedById;
            request.DateActioned = DateTime.UtcNow;
            await _leaveRequestRepository.UpdateAsync(request);

            _logger.LogInformation("Leave moved to admin approval. LeaveRequestId: {LeaveRequestId}, ApprovedById: {ApprovedById}",
                leaveRequestId, approvedById);
            CompactCache();
            await _notificationService.SendAsync(request.RequestingEmployeeId, "Leave request moved to final approval", "Your leave request is now pending admin approval.");
            return ServiceResult.Ok("Leave request forwarded to Admin for final approval.");
        }

        var result = await _leaveRequestRepository.ApproveBySpAsync(leaveRequestId, approvedById, comments);
        if (result.Result == 1)
        {
            _logger.LogInformation("Leave approved. LeaveRequestId: {LeaveRequestId}, ApprovedById: {ApprovedById}", leaveRequestId, approvedById);
            CompactCache();
            await _notificationService.SendAsync(request.RequestingEmployeeId, "Leave request approved", result.Message);
            return ServiceResult.Ok(result.Message);
        }
        return ServiceResult.Fail(result.Message);
    }

    public async Task<ServiceResult> RejectLeaveAsync(int leaveRequestId, string rejectedById, string comments)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (request == null) return ServiceResult.Fail("Leave request not found.");

        var result = await _leaveRequestRepository.RejectBySpAsync(leaveRequestId, rejectedById, comments);
        if (result.Result == 1)
        {
            _logger.LogInformation("Leave rejected. LeaveRequestId: {LeaveRequestId}, RejectedById: {RejectedById}", leaveRequestId, rejectedById);
            CompactCache();
            await _notificationService.SendAsync(request.RequestingEmployeeId, "Leave request rejected", result.Message);
            return ServiceResult.Ok(result.Message);
        }
        return ServiceResult.Fail(result.Message);
    }

    public async Task<ServiceResult> CancelLeaveAsync(int leaveRequestId, string userId)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(leaveRequestId);
        if (request == null || request.RequestingEmployeeId != userId || request.Cancelled)
            return ServiceResult.Fail("Leave request not found for user.");

        if (!string.Equals(request.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return ServiceResult.Fail("Only pending leave requests can be cancelled.");

        var result = await _leaveRequestRepository.CancelBySpAsync(leaveRequestId, userId);
        if (result.Result == 1) CompactCache();
        return result.Result == 1
            ? ServiceResult.Ok(result.Message)
            : ServiceResult.Fail(result.Message);
    }

    public async Task<List<LeaveBalanceVM>> GetLeaveBalanceAsync(string userId, int year)
    {
        var balances = await _leaveAllocationRepository.GetLeaveBalanceBySpAsync(userId, year);
        var requests = await _leaveRequestRepository.GetByEmployeeAsync(userId, year);

        var pendingByLeaveType = requests
            .Where(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.LeaveTypeId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.NumberOfDays));

        foreach (var balance in balances)
        {
            if (!pendingByLeaveType.TryGetValue(balance.LeaveTypeId, out var pendingDays))
                continue;

            balance.RemainingDays = Math.Max(0, balance.RemainingDays - pendingDays);
        }

        return balances;
    }

    private static int CalculateBillableDays(ApplyLeaveVM model, List<Holiday> holidays)
    {
        if (model.IsHalfDay)
        {
            if (model.StartDate.Date != model.EndDate.Date)
                return 0;
            if (model.StartDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return 0;
            if (holidays.Any(h => h.HolidayDate.Date == model.StartDate.Date))
                return 0;
            return 1;
        }

        var workingDays = LeaveCalculationHelper.CalculateWorkingDays(model.StartDate, model.EndDate, holidays);
        if (workingDays <= 0) return workingDays;

        // Sandwich rule: weekend days between working leave dates are counted as leave.
        var weekendDays = Enumerable.Range(0, (model.EndDate.Date - model.StartDate.Date).Days + 1)
            .Select(offset => model.StartDate.Date.AddDays(offset))
            .Where(date => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            .Where(date =>
                date > model.StartDate.Date &&
                date < model.EndDate.Date &&
                !holidays.Any(h => h.HolidayDate.Date == date))
            .ToList();

        return workingDays + weekendDays.Count;
    }

    private void CompactCache()
    {
        if (_cache is MemoryCache memoryCache)
            memoryCache.Compact(0.25);
    }
}
