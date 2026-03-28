using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<List<AuditLog>> GetRecentAsync(int take = 200);
}
