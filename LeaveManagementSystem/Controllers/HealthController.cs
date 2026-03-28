using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class HealthController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> DbObjects()
    {
        try
        {
            var expectedTables = new[]
            {
                "Departments", "AspNetUsers", "LeaveTypes", "LeaveAllocations",
                "LeaveRequests", "Holidays", "AuditLogs"
            };

            var expectedProcedures = new[]
            {
                "sp_GetLeaveRequests",
                "sp_GetLeaveRequestsByEmployee",
                "sp_GetPendingRequestsForManager",
                "sp_ApplyLeave",
                "sp_ApproveLeave",
                "sp_RejectLeave",
                "sp_CancelLeave",
                "sp_GetLeaveBalance",
                "sp_AllocateLeave",
                "sp_GetAdminDashboardStats",
                "sp_GetManagerDashboardStats",
                "sp_GetLeaveReport"
            };

            var foundTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var foundProcedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using var conn = (SqlConnection)_context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            var sql = @"
SELECT name, type
FROM sys.objects
WHERE (type = 'U' AND name IN (
    'Departments','AspNetUsers','LeaveTypes','LeaveAllocations','LeaveRequests','Holidays','AuditLogs'
))
OR (type = 'P' AND name IN (
    'sp_GetLeaveRequests','sp_GetLeaveRequestsByEmployee','sp_GetPendingRequestsForManager','sp_ApplyLeave',
    'sp_ApproveLeave','sp_RejectLeave','sp_CancelLeave','sp_GetLeaveBalance','sp_AllocateLeave',
    'sp_GetAdminDashboardStats','sp_GetManagerDashboardStats','sp_GetLeaveReport'
));";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var name = reader["name"]?.ToString() ?? string.Empty;
                var type = reader["type"]?.ToString() ?? string.Empty;
                if (type == "U") foundTables.Add(name);
                if (type == "P") foundProcedures.Add(name);
            }

            var missingTables = expectedTables.Where(x => !foundTables.Contains(x)).ToList();
            var missingProcedures = expectedProcedures.Where(x => !foundProcedures.Contains(x)).ToList();

            return Json(new
            {
                ok = missingTables.Count == 0 && missingProcedures.Count == 0,
                expected = new
                {
                    tables = expectedTables,
                    procedures = expectedProcedures
                },
                found = new
                {
                    tables = foundTables.OrderBy(x => x).ToList(),
                    procedures = foundProcedures.OrderBy(x => x).ToList()
                },
                missing = new
                {
                    tables = missingTables,
                    procedures = missingProcedures
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB object health check failed.");
            return StatusCode(500, new { ok = false, message = "DB object health check failed." });
        }
    }
}
