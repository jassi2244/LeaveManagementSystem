using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> LeaveReport(ReportFilterVM filter)
    {
        try
        {
            var managerId = User.IsInRole("Manager")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var report = await _reportService.GetLeaveReportAsync(filter, managerId);
            filter.Records = report.Select(x => new LeaveRequestVM
            {
                EmployeeName = x.EmployeeName,
                LeaveTypeName = x.LeaveTypeName,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                NumberOfDays = x.NumberOfDays,
                Status = x.Status
            }).ToList();
            return View(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report load failed");
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExportToExcel(ReportFilterVM filter)
    {
        try
        {
            ExcelPackage.License.SetNonCommercialOrganization("LeaveManagementSystem");
            var managerId = User.IsInRole("Manager")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var report = await _reportService.GetLeaveReportAsync(filter, managerId);
            const int maxExportRows = 5000;
            if (report.Count > maxExportRows)
            {
                TempData["Warning"] = $"Export limit exceeded. Please narrow filters to {maxExportRows} rows or fewer.";
                return RedirectToAction(nameof(LeaveReport), filter);
            }
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("LeaveReport");
            ws.Cells[1, 1].Value = "Employee";
            ws.Cells[1, 2].Value = "Department";
            ws.Cells[1, 3].Value = "Leave Type";
            ws.Cells[1, 4].Value = "Start";
            ws.Cells[1, 5].Value = "End";
            ws.Cells[1, 6].Value = "Days";
            ws.Cells[1, 7].Value = "Status";
            for (var i = 0; i < report.Count; i++)
            {
                var row = i + 2;
                ws.Cells[row, 1].Value = report[i].EmployeeName;
                ws.Cells[row, 2].Value = report[i].DepartmentName;
                ws.Cells[row, 3].Value = report[i].LeaveTypeName;
                ws.Cells[row, 4].Value = report[i].StartDate.ToShortDateString();
                ws.Cells[row, 5].Value = report[i].EndDate.ToShortDateString();
                ws.Cells[row, 6].Value = report[i].NumberOfDays;
                ws.Cells[row, 7].Value = report[i].Status;
            }
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LeaveReport.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed");
            return RedirectToAction(nameof(LeaveReport));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExportToPdf(ReportFilterVM filter)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var managerId = User.IsInRole("Manager")
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var rows = await _reportService.GetLeaveReportAsync(filter, managerId);
            const int maxExportRows = 2000;
            if (rows.Count > maxExportRows)
            {
                TempData["Warning"] = $"PDF export limit exceeded. Please narrow filters to {maxExportRows} rows or fewer.";
                return RedirectToAction(nameof(LeaveReport), filter);
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(24);
                    page.Header().Text("Leave Report").FontSize(18).SemiBold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Employee").SemiBold();
                            header.Cell().Text("Department").SemiBold();
                            header.Cell().Text("Leave Type").SemiBold();
                            header.Cell().Text("Range").SemiBold();
                            header.Cell().Text("Days").SemiBold();
                            header.Cell().Text("Status").SemiBold();
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Text(row.EmployeeName);
                            table.Cell().Text(row.DepartmentName);
                            table.Cell().Text(row.LeaveTypeName);
                            table.Cell().Text($"{row.StartDate:dd MMM yyyy} - {row.EndDate:dd MMM yyyy}");
                            table.Cell().Text(row.NumberOfDays.ToString());
                            table.Cell().Text(row.Status);
                        }
                    });
                    page.Footer().AlignRight().Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "LeaveReport.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF export failed");
            TempData["Error"] = "Unable to export PDF.";
            return RedirectToAction(nameof(LeaveReport), filter);
        }
    }

    [HttpGet]
    public async Task<IActionResult> LeaveReportSummary(ReportFilterVM filter)
    {
        var managerId = User.IsInRole("Manager")
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
        var summary = await _reportService.GetReportSummaryAsync(filter, managerId);
        return Json(summary);
    }
}
