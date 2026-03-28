using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveRequest>> GetAllAsync()
    {
        var items = new List<LeaveRequest>();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetLeaveRequests", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
        cmd.Parameters.AddWithValue("@DepartmentId", DBNull.Value);
        cmd.Parameters.AddWithValue("@LeaveTypeId", DBNull.Value);
        cmd.Parameters.AddWithValue("@Year", DBNull.Value);
        cmd.Parameters.AddWithValue("@UserId", DBNull.Value);
        cmd.Parameters.AddWithValue("@PageNumber", 1);
        cmd.Parameters.AddWithValue("@PageSize", 500);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new LeaveRequest
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                NumberOfDays = reader.GetInt32(reader.GetOrdinal("NumberOfDays")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                DateRequested = reader.GetDateTime(reader.GetOrdinal("DateRequested")),
                Cancelled = reader.GetBoolean(reader.GetOrdinal("Cancelled")),
                RequestingEmployee = new ApplicationUser
                {
                    FullName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                    Department = new Department { Name = reader["DepartmentName"]?.ToString() ?? string.Empty }
                },
                LeaveType = new LeaveType
                {
                    Name = reader["LeaveTypeName"]?.ToString() ?? string.Empty
                }
            });
        }

        return items;
    }

    public async Task<List<LeaveRequest>> GetRecentPendingForAdminAsync(int take)
    {
        var items = new List<LeaveRequest>();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetLeaveRequests", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Status", "Pending");
        cmd.Parameters.AddWithValue("@DepartmentId", DBNull.Value);
        cmd.Parameters.AddWithValue("@LeaveTypeId", DBNull.Value);
        cmd.Parameters.AddWithValue("@Year", DBNull.Value);
        cmd.Parameters.AddWithValue("@UserId", DBNull.Value);
        cmd.Parameters.AddWithValue("@PageNumber", 1);
        cmd.Parameters.AddWithValue("@PageSize", Math.Max(1, take));

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new LeaveRequest
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                NumberOfDays = reader.GetInt32(reader.GetOrdinal("NumberOfDays")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                DateRequested = reader.GetDateTime(reader.GetOrdinal("DateRequested")),
                Cancelled = reader.GetBoolean(reader.GetOrdinal("Cancelled")),
                RequestingEmployee = new ApplicationUser
                {
                    FullName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                    Department = new Department { Name = reader["DepartmentName"]?.ToString() ?? string.Empty }
                },
                LeaveType = new LeaveType
                {
                    Name = reader["LeaveTypeName"]?.ToString() ?? string.Empty
                }
            });
        }

        return items;
    }

    public async Task<List<LeaveRequest>> GetByEmployeeAsync(string userId, int? year = null)
    {
        var items = new List<LeaveRequest>();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetLeaveRequestsByEmployee", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Year", year.HasValue ? year.Value : DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new LeaveRequest
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                NumberOfDays = reader.GetInt32(reader.GetOrdinal("NumberOfDays")),
                Status = reader["Status"]?.ToString() ?? string.Empty,
                DateRequested = reader.GetDateTime(reader.GetOrdinal("DateRequested")),
                Reason = reader["Reason"]?.ToString(),
                LeaveType = new LeaveType { Name = reader["LeaveTypeName"]?.ToString() ?? string.Empty }
            });
        }
        return items;
    }

    public async Task<List<LeaveRequest>> GetPendingForManagerAsync(string managerId)
    {
        var items = new List<LeaveRequest>();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetPendingRequestsForManager", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ManagerId", managerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new LeaveRequest
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                NumberOfDays = reader.GetInt32(reader.GetOrdinal("NumberOfDays")),
                DateRequested = reader.GetDateTime(reader.GetOrdinal("DateRequested")),
                Status = "Pending",
                RequestingEmployee = new ApplicationUser
                {
                    FullName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                    Department = new Department { Name = reader["DepartmentName"]?.ToString() ?? string.Empty }
                },
                LeaveType = new LeaveType
                {
                    Name = reader["LeaveTypeName"]?.ToString() ?? string.Empty
                }
            });
        }

        return items;
    }

    public async Task<LeaveRequest?> GetByIdAsync(int id) =>
        await _context.LeaveRequests
            .Include(x => x.LeaveType)
            .Include(x => x.RequestingEmployee)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(LeaveRequest request)
    {
        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveRequest request)
    {
        _context.LeaveRequests.Update(request);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOverlappingAsync(string userId, DateTime start, DateTime end) =>
        await _context.LeaveRequests.AnyAsync(x =>
            x.RequestingEmployeeId == userId &&
            !x.Cancelled &&
            (x.Status == "Pending" || x.Status == "PendingAdminApproval" || x.Status == "Approved") &&
            start <= x.EndDate && end >= x.StartDate);

    public async Task<(int Result, string Message)> ApplyBySpAsync(string userId, int leaveTypeId, DateTime startDate, DateTime endDate, int numberOfDays, string? reason)
    {
        var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_ApplyLeave @UserId, @LeaveTypeId, @StartDate, @EndDate, @NumberOfDays, @Reason, @Result OUTPUT, @Message OUTPUT",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@LeaveTypeId", leaveTypeId),
            new SqlParameter("@StartDate", startDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@NumberOfDays", numberOfDays),
            new SqlParameter("@Reason", (object?)reason ?? DBNull.Value),
            pResult,
            pMessage);
        return (pResult.Value is int r ? r : 0, pMessage.Value?.ToString() ?? "No message");
    }

    public async Task<(int Result, string Message)> ApproveBySpAsync(int leaveRequestId, string approvedById, string? comments)
    {
        var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_ApproveLeave @LeaveRequestId, @ApprovedById, @Comments, @Result OUTPUT, @Message OUTPUT",
            new SqlParameter("@LeaveRequestId", leaveRequestId),
            new SqlParameter("@ApprovedById", approvedById),
            new SqlParameter("@Comments", (object?)comments ?? DBNull.Value),
            pResult,
            pMessage);
        return (pResult.Value is int r ? r : 0, pMessage.Value?.ToString() ?? "No message");
    }

    public async Task<(int Result, string Message)> RejectBySpAsync(int leaveRequestId, string rejectedById, string comments)
    {
        var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RejectLeave @LeaveRequestId, @RejectedById, @Comments, @Result OUTPUT, @Message OUTPUT",
            new SqlParameter("@LeaveRequestId", leaveRequestId),
            new SqlParameter("@RejectedById", rejectedById),
            new SqlParameter("@Comments", comments),
            pResult,
            pMessage);
        return (pResult.Value is int r ? r : 0, pMessage.Value?.ToString() ?? "No message");
    }

    public async Task<(int Result, string Message)> CancelBySpAsync(int leaveRequestId, string userId)
    {
        var pResult = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_CancelLeave @LeaveRequestId, @UserId, @Result OUTPUT, @Message OUTPUT",
            new SqlParameter("@LeaveRequestId", leaveRequestId),
            new SqlParameter("@UserId", userId),
            pResult,
            pMessage);
        return (pResult.Value is int r ? r : 0, pMessage.Value?.ToString() ?? "No message");
    }
}
