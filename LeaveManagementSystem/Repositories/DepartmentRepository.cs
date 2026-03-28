using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;
    public DepartmentRepository(ApplicationDbContext context) => _context = context;

    public async Task<List<Department>> GetAllAsync() =>
        await _context.Departments
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    public async Task<Department?> GetByIdAsync(int id) => await _context.Departments.FindAsync(id);
    public async Task<bool> IsInUseAsync(int id) =>
        await _context.Users.AnyAsync(x => x.DepartmentId == id && x.IsActive);
    public async Task AddAsync(Department department) { _context.Departments.Add(department); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Department department)
    {
        var entity = await _context.Departments.FindAsync(department.Id);
        if (entity == null) return;

        entity.Name = department.Name;
        entity.Description = department.Description;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Departments.FindAsync(id);
        if (entity == null) return;

        entity.IsActive = false;
        await _context.SaveChangesAsync();
    }
}
