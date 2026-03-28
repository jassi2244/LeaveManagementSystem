using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;
    public AuditLogRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public Task<List<AuditLog>> GetRecentAsync(int take = 200)
    {
        return _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .Take(Math.Max(1, take))
            .ToListAsync();
    }
}
