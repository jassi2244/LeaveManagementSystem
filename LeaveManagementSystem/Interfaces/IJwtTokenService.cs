using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Interfaces;

public interface IJwtTokenService
{
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(ApplicationUser user);
}
