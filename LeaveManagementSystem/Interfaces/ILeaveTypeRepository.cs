using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Interfaces;

public interface ILeaveTypeRepository
{
    Task<List<LeaveType>> GetAllAsync();
    Task<LeaveType?> GetByIdAsync(int id);
    Task<bool> IsInUseAsync(int id);
    Task AddAsync(LeaveType leaveType);
    Task UpdateAsync(LeaveType leaveType);
    Task DeleteAsync(int id);
}
