using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface ILeaveAllocationService
{
    Task<ServiceResult> AllocateAsync(LeaveAllocationVM model, string performedBy);
}
