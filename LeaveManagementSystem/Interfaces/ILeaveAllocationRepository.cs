using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Interfaces;

public interface ILeaveAllocationRepository
{
    Task<LeaveAllocation?> GetAsync(string userId, int leaveTypeId, int year);
    Task<List<LeaveAllocation>> GetByUserAsync(string userId, int year);
    Task<List<LeaveBalanceVM>> GetLeaveBalanceBySpAsync(string userId, int year);
    Task<(int Result, string Message)> AllocateBySpAsync(string userId, int leaveTypeId, int numberOfDays, int year);
    Task<int> ResetYearlyUsedDaysAsync(int year);
    Task AddAsync(LeaveAllocation allocation);
    Task UpdateAsync(LeaveAllocation allocation);
}
