using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditLogRepository auditLogRepository,
        ApplicationDbContext context,
        ILogger<AuditController> logger)
    {
        _auditLogRepository = auditLogRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var logs = await _auditLogRepository.GetRecentAsync(300);
            var performerIds = logs
                .Select(x => x.PerformedBy)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !string.Equals(x, "SYSTEM", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            var performers = await _context.Users
                .AsNoTracking()
                .Where(x => performerIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.Email })
                .ToDictionaryAsync(
                    x => x.Id,
                    x => string.IsNullOrWhiteSpace(x.FullName) ? (x.Email ?? x.Id) : $"{x.FullName} ({x.Email})");

            ViewBag.PerformerDisplayNames = performers;
            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load audit logs.");
            TempData["Error"] = "Unable to load audit logs.";
            return View(new List<Models.AuditLog>());
        }
    }
}
