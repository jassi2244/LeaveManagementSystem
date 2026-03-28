using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Interfaces;

public interface IHolidayRepository
{
    Task<List<Holiday>> GetAllAsync();
    Task<List<Holiday>> GetBetweenAsync(DateTime start, DateTime end);
    Task<Holiday?> GetByIdAsync(int id);
    Task AddAsync(Holiday holiday);
    Task UpdateAsync(Holiday holiday);
    Task DeleteAsync(int id);
}
