using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Repositories;

public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string LeaveTypesCacheKey = "ref:leave-types:active";
    public LeaveTypeRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<LeaveType>> GetAllAsync() =>
        await _cache.GetOrCreateAsync(LeaveTypesCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.LeaveTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .AsNoTracking()
                .ToListAsync();
        }) ?? [];
    public async Task<LeaveType?> GetByIdAsync(int id) => await _context.LeaveTypes.FindAsync(id);
    public async Task<bool> IsInUseAsync(int id) =>
        await _context.LeaveAllocations.AnyAsync(x => x.LeaveTypeId == id)
        || await _context.LeaveRequests.AnyAsync(x => x.LeaveTypeId == id);
    public async Task AddAsync(LeaveType leaveType)
    {
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();
        _cache.Remove(LeaveTypesCacheKey);
    }
    public async Task UpdateAsync(LeaveType leaveType)
    {
        var entity = await _context.LeaveTypes.FindAsync(leaveType.Id);
        if (entity == null) return;

        entity.Name = leaveType.Name;
        entity.DefaultDays = leaveType.DefaultDays;
        entity.Description = leaveType.Description;
        await _context.SaveChangesAsync();
        _cache.Remove(LeaveTypesCacheKey);
    }
    public async Task DeleteAsync(int id)
    {
        var entity = await _context.LeaveTypes.FindAsync(id);
        if (entity == null) return;

        entity.IsActive = false;
        await _context.SaveChangesAsync();
        _cache.Remove(LeaveTypesCacheKey);
    }
}
