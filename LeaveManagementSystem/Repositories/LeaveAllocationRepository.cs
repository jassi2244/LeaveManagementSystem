using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Repositories;

public class LeaveAllocationRepository : ILeaveAllocationRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveAllocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveAllocation?> GetAsync(string userId, int leaveTypeId, int year) =>
        await _context.LeaveAllocations.FirstOrDefaultAsync(x =>
            x.UserId == userId && x.LeaveTypeId == leaveTypeId && x.Period == year);

    public async Task<List<LeaveAllocation>> GetByUserAsync(string userId, int year) =>
        await _context.LeaveAllocations.Include(x => x.LeaveType)
            .Where(x => x.UserId == userId && x.Period == year)
            .ToListAsync();

    public async Task<List<LeaveBalanceVM>> GetLeaveBalanceBySpAsync(string userId, int year)
    {
        var list = new List<LeaveBalanceVM>();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand("sp_GetLeaveBalance", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Year", year);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LeaveBalanceVM
            {
                LeaveTypeId = Convert.ToInt32(reader["LeaveTypeId"]),
                LeaveTypeName = reader["LeaveTypeName"]?.ToString() ?? string.Empty,
                TotalDays = Convert.ToInt32(reader["TotalDays"]),
                UsedDays = Convert.ToInt32(reader["UsedDays"]),
                RemainingDays = Convert.ToInt32(reader["RemainingDays"])
            });
        }

        return list;
    }

    public async Task<(int Result, string Message)> AllocateBySpAsync(string userId, int leaveTypeId, int numberOfDays, int year)
    {
        var pUserId = new SqlParameter("@UserId", userId);
        var pLeaveTypeId = new SqlParameter("@LeaveTypeId", leaveTypeId);
        var pNumberOfDays = new SqlParameter("@NumberOfDays", numberOfDays);
        var pYear = new SqlParameter("@Year", year);
        var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_AllocateLeave @UserId, @LeaveTypeId, @NumberOfDays, @Year, @Result OUTPUT, @Message OUTPUT",
            pUserId, pLeaveTypeId, pNumberOfDays, pYear, pResult, pMessage);

        return (pResult.Value is int r ? r : 0, pMessage.Value?.ToString() ?? "No message");
    }

    public async Task<int> ResetYearlyUsedDaysAsync(int year)
    {
        return await _context.Database.ExecuteSqlRawAsync(
            "UPDATE LeaveAllocations SET UsedDays = 0 WHERE Period = @p0",
            year);
    }

    public async Task AddAsync(LeaveAllocation allocation)
    {
        _context.LeaveAllocations.Add(allocation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveAllocation allocation)
    {
        _context.LeaveAllocations.Update(allocation);
        await _context.SaveChangesAsync();
    }
}
