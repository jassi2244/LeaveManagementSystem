using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface ILeaveRequestRepository
{
    Task<List<LeaveRequest>> GetAllAsync();
    Task<List<LeaveRequest>> GetRecentPendingForAdminAsync(int take);
    Task<List<LeaveRequest>> GetByEmployeeAsync(string userId, int? year = null);
    Task<List<LeaveRequest>> GetPendingForManagerAsync(string managerId);
    Task<LeaveRequest?> GetByIdAsync(int id);
    Task AddAsync(LeaveRequest request);
    Task UpdateAsync(LeaveRequest request);
    Task<bool> HasOverlappingAsync(string userId, DateTime start, DateTime end);
    Task<(int Result, string Message)> ApplyBySpAsync(string userId, int leaveTypeId, DateTime startDate, DateTime endDate, int numberOfDays, string? reason);
    Task<(int Result, string Message)> ApproveBySpAsync(int leaveRequestId, string approvedById, string? comments);
    Task<(int Result, string Message)> RejectBySpAsync(int leaveRequestId, string rejectedById, string comments);
    Task<(int Result, string Message)> CancelBySpAsync(int leaveRequestId, string userId);
}
