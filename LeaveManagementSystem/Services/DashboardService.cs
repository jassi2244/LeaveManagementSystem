using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IMemoryCache _cache;

    public DashboardService(
        ApplicationDbContext context,
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveRequestService leaveRequestService,
        IMemoryCache cache)
    {
        _context = context;
        _leaveRequestRepository = leaveRequestRepository;
        _leaveRequestService = leaveRequestService;
        _cache = cache;
    }

    public async Task<DashboardAdminVM> GetAdminDashboardAsync(int year)
    {
        var cacheKey = $"dashboard:admin:{year}";
        if (_cache.TryGetValue(cacheKey, out DashboardAdminVM? cached) && cached != null)
            return cached;

        var vm = new DashboardAdminVM();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetAdminDashboardStats", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Year", year);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            vm.TotalEmployees = Convert.ToInt32(reader["TotalEmployees"]);
            vm.TotalPendingRequests = Convert.ToInt32(reader["TotalPendingRequests"]);
            vm.TotalApprovedThisMonth = Convert.ToInt32(reader["TotalApprovedThisMonth"]);
            vm.TotalRejectedThisMonth = Convert.ToInt32(reader["TotalRejectedThisMonth"]);
        }
        await reader.CloseAsync();

        var pending = await _leaveRequestRepository.GetRecentPendingForAdminAsync(5);
        vm.RecentPendingRequests = pending
            .Select(x => new LeaveRequestVM
            {
                Id = x.Id,
                EmployeeName = x.RequestingEmployee?.FullName ?? string.Empty,
                LeaveTypeName = x.LeaveType?.Name ?? string.Empty,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                NumberOfDays = x.NumberOfDays,
                Status = x.Status
            }).ToList();

        _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(3));
        return vm;
    }

    public async Task<DashboardManagerVM> GetManagerDashboardAsync(string managerId, int year)
    {
        var cacheKey = $"dashboard:manager:{managerId}:{year}";
        if (_cache.TryGetValue(cacheKey, out DashboardManagerVM? cached) && cached != null)
            return cached;

        var vm = new DashboardManagerVM();
        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetManagerDashboardStats", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ManagerId", managerId);
        cmd.Parameters.AddWithValue("@Year", year);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            vm.TeamSize = Convert.ToInt32(reader["TeamSize"]);
            vm.PendingApprovals = Convert.ToInt32(reader["PendingApprovals"]);
            vm.ApprovedThisMonth = Convert.ToInt32(reader["ApprovedThisMonth"]);
        }
        await reader.CloseAsync();

        var pending = await _leaveRequestRepository.GetPendingForManagerAsync(managerId);
        vm.TeamPendingRequests = pending.Select(x => new LeaveRequestVM
        {
            Id = x.Id,
            EmployeeName = x.RequestingEmployee?.FullName ?? "",
            LeaveTypeName = x.LeaveType?.Name ?? "",
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            NumberOfDays = x.NumberOfDays,
            Status = x.Status
        }).ToList();
        _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(3));
        return vm;
    }

    public async Task<DashboardEmployeeVM> GetEmployeeDashboardAsync(string userId, int year)
    {
        var cacheKey = $"dashboard:employee:{userId}:{year}";
        if (_cache.TryGetValue(cacheKey, out DashboardEmployeeVM? cached) && cached != null)
            return cached;

        var balances = await _leaveRequestService.GetLeaveBalanceAsync(userId, year);
        var requests = await _leaveRequestRepository.GetByEmployeeAsync(userId, year);
        var vm = new DashboardEmployeeVM
        {
            LeaveBalances = balances,
            RecentRequests = requests.Take(5).Select(x => new LeaveRequestVM
            {
                Id = x.Id,
                LeaveTypeName = x.LeaveType?.Name ?? "",
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                NumberOfDays = x.NumberOfDays,
                Status = x.Status
            }).ToList()
        };
        _cache.Set(cacheKey, vm, TimeSpan.FromMinutes(2));
        return vm;
    }

    public async Task<DashboardChartDataVM> GetAdminChartDataAsync(int year)
    {
        var cacheKey = $"dashboard:chart:admin:{year}";
        if (_cache.TryGetValue(cacheKey, out DashboardChartDataVM? cached) && cached != null)
            return cached;

        var connectionString = _context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Database connection string is not configured.");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("sp_GetAdminDashboardStats", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Year", year);

        var chart = new DashboardChartDataVM();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            chart.PendingCount = Convert.ToInt32(reader["TotalPendingRequests"]);
            chart.ApprovedCount = Convert.ToInt32(reader["TotalApprovedThisMonth"]);
            chart.RejectedCount = Convert.ToInt32(reader["TotalRejectedThisMonth"]);
        }
        await reader.NextResultAsync();
        await reader.NextResultAsync();
        var monthly = Enumerable.Repeat(0, 12).ToArray();
        while (await reader.ReadAsync())
        {
            var month = Convert.ToInt32(reader["Month"]);
            var total = Convert.ToInt32(reader["TotalRequests"]);
            if (month >= 1 && month <= 12) monthly[month - 1] = total;
        }

        chart.MonthlyRequestCounts = monthly.ToList();
        _cache.Set(cacheKey, chart, TimeSpan.FromMinutes(3));
        return chart;
    }
}
