using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface ILeaveRequestService
{
    Task<ServiceResult> ApplyLeaveAsync(ApplyLeaveVM model, string userId);
    Task<ServiceResult> ApproveLeaveAsync(int leaveRequestId, string approvedById, string? comments);
    Task<ServiceResult> RejectLeaveAsync(int leaveRequestId, string rejectedById, string comments);
    Task<ServiceResult> CancelLeaveAsync(int leaveRequestId, string userId);
    Task<List<LeaveBalanceVM>> GetLeaveBalanceAsync(string userId, int year);
}
