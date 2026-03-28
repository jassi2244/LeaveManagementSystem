using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;

namespace LeaveManagementSystem.Services;

public class LeaveAllocationService : ILeaveAllocationService
{
    private readonly ILeaveAllocationRepository _repo;

    public LeaveAllocationService(ILeaveAllocationRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResult> AllocateAsync(LeaveAllocationVM model, string performedBy)
    {
        var spResult = await _repo.AllocateBySpAsync(model.UserId, model.LeaveTypeId, model.NumberOfDays, model.Year);
        if (spResult.Result == 0)
            return ServiceResult.Fail(spResult.Message);
        return ServiceResult.Ok(spResult.Message);
    }
}
