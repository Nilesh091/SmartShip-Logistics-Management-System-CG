using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;

namespace AuthService.API.Controllers
{
  [ApiController]
  [Route("api/auth")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
      _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      try
      {
        var result = await _authService.Register(dto);
        return Ok(new { message = result.AccessToken, status = "OTP sent to email" });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
      try
      {
        var result = await _authService.Login(dto);
        return Ok(new { message = result, status = "OTP sent to email" });
      }
      catch (Exception ex)
      {
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
      try
      {
        var result = await _authService.VerifyOtp(dto);
        return Ok(new { success = true, message = "OTP verified successfully", data = result });
      }
      catch (Exception ex)
      {
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromQuery] string token)
    {
      try
      {
        var result = await _authService.RefreshToken(token);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromQuery] string token)
    {
      try
      {
        var result = await _authService.RevokeToken(token);
        return Ok(new { success = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpGet("admin/users")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllUsers()
    {
      try
      {
        var users = await _authService.GetAllUsersAsync();
        return Ok(new { success = true, message = "Users retrieved successfully", data = users });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpPut("admin/users/{userId}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequestDto dto)
    {
      try
      {
        var result = await _authService.UpdateUserRoleAsync(userId, dto);
        return Ok(new { success = true, message = "User role updated successfully", data = result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = ex.Message });
      }
    }
  }
}

