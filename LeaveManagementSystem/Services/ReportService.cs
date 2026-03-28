using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveReportDTO>> GetLeaveReportAsync(ReportFilterVM filter, string? managerId = null)
    {
        var list = new List<LeaveReportDTO>();
        await using var conn = (SqlConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = new SqlCommand("sp_GetLeaveReport", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@DepartmentId", (object?)filter.DepartmentId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LeaveTypeId", (object?)filter.LeaveTypeId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Status", (object?)filter.Status ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FromDate", (object?)filter.FromDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ToDate", (object?)filter.ToDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Year", (object?)filter.Year ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ManagerId", (object?)managerId ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new LeaveReportDTO
            {
                EmployeeName = reader["EmployeeName"]?.ToString() ?? string.Empty,
                DepartmentName = reader["DepartmentName"]?.ToString() ?? string.Empty,
                LeaveTypeName = reader["LeaveTypeName"]?.ToString() ?? string.Empty,
                StartDate = Convert.ToDateTime(reader["StartDate"]),
                EndDate = Convert.ToDateTime(reader["EndDate"]),
                NumberOfDays = Convert.ToInt32(reader["NumberOfDays"]),
                Status = reader["Status"]?.ToString() ?? string.Empty,
                ApprovedBy = reader["ApprovedBy"]?.ToString()
            });
        }

        if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
        {
            list = list
                .Where(x => x.EmployeeName.Contains(filter.EmployeeName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return list;
    }

    public async Task<ReportSummaryDTO> GetReportSummaryAsync(ReportFilterVM filter, string? managerId = null)
    {
        var rows = await GetLeaveReportAsync(filter, managerId);
        return new ReportSummaryDTO
        {
            Total = rows.Count,
            Approved = rows.Count(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase)),
            Pending = rows.Count(x => string.Equals(x.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(x.Status, "PendingAdminApproval", StringComparison.OrdinalIgnoreCase)),
            Rejected = rows.Count(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)),
            Cancelled = rows.Count(x => string.Equals(x.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        };
    }
}
