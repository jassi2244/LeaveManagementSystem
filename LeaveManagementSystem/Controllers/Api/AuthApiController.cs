using LeaveManagementSystem.DTOs.Api;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers.Api;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class AuthApiController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthApiController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("token")]
    public async Task<ActionResult<AuthTokenResponseDTO>> GetToken([FromBody] AuthLoginRequestDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials." });

        var (token, expiresAtUtc) = await _jwtTokenService.GenerateTokenAsync(user);
        return Ok(new AuthTokenResponseDTO { AccessToken = token, ExpiresAtUtc = expiresAtUtc });
    }
}
