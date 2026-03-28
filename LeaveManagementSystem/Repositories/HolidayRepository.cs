using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Repositories;

public class HolidayRepository : IHolidayRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string HolidaysCacheKey = "ref:holidays:all";
    public HolidayRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<Holiday>> GetAllAsync() =>
        await _cache.GetOrCreateAsync(HolidaysCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _context.Holidays
                .OrderBy(x => x.HolidayDate)
                .AsNoTracking()
                .ToListAsync();
        }) ?? [];
    public async Task<List<Holiday>> GetBetweenAsync(DateTime start, DateTime end) =>
        await _context.Holidays.Where(x => x.HolidayDate.Date >= start.Date && x.HolidayDate.Date <= end.Date).ToListAsync();
    public async Task<Holiday?> GetByIdAsync(int id) => await _context.Holidays.FindAsync(id);
    public async Task AddAsync(Holiday holiday)
    {
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync();
        _cache.Remove(HolidaysCacheKey);
    }
    public async Task UpdateAsync(Holiday holiday)
    {
        _context.Holidays.Update(holiday);
        await _context.SaveChangesAsync();
        _cache.Remove(HolidaysCacheKey);
    }
    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Holidays.FindAsync(id);
        if (entity != null)
        {
            _context.Holidays.Remove(entity);
            await _context.SaveChangesAsync();
            _cache.Remove(HolidaysCacheKey);
        }
    }
}
