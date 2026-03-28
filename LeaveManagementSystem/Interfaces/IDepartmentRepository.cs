using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Interfaces;

public interface IDepartmentRepository
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<bool> IsInUseAsync(int id);
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);
}
