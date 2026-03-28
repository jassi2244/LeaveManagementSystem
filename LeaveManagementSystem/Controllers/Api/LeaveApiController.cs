using AutoMapper;
using LeaveManagementSystem.DTOs.Api;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagementSystem.Controllers.Api;

[ApiController]
[Route("api/leave")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken]
public class LeaveApiController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IMapper _mapper;

    public LeaveApiController(ILeaveRequestService leaveRequestService, IMapper mapper)
    {
        _leaveRequestService = leaveRequestService;
        _mapper = mapper;
    }

    [HttpPost("apply")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> Apply([FromBody] ApplyLeaveVM request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _leaveRequestService.ApplyLeaveAsync(request, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{leaveRequestId:int}/approve")]
    [Authorize(Policy = "CanApproveLeave", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Approve(int leaveRequestId, [FromBody] ApproveRejectLeaveApiDTO request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _leaveRequestService.ApproveLeaveAsync(leaveRequestId, userId, request.Comments);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{leaveRequestId:int}/reject")]
    [Authorize(Policy = "CanApproveLeave", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Reject(int leaveRequestId, [FromBody] ApproveRejectLeaveApiDTO request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var comments = string.IsNullOrWhiteSpace(request.Comments) ? "Rejected from API" : request.Comments;
        var result = await _leaveRequestService.RejectLeaveAsync(leaveRequestId, userId, comments);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("balance")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<IActionResult> GetBalance([FromQuery] int? year)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var rows = await _leaveRequestService.GetLeaveBalanceAsync(userId, year ?? DateTime.UtcNow.Year);
        return Ok(rows.Select(_mapper.Map<LeaveManagementSystem.DTOs.LeaveBalanceDTO>));
    }
}
