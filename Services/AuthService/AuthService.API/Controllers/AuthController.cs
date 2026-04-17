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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
      _authService = authService;
      _logger = logger;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
      try
      {
        _logger.LogInformation($"User registration attempt for email: {dto.Email}");
        var result = await _authService.Register(dto);
        _logger.LogInformation($"User registered successfully: {dto.Email}");
        return Ok(new { message = result.AccessToken, status = "OTP sent to email" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Registration error for email {dto.Email}: {ex.Message}");
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
      try
      {
        _logger.LogInformation($"Login attempt for email: {dto.Email}");
        var result = await _authService.Login(dto);
        _logger.LogInformation($"Login successful for email: {dto.Email}");
        return Ok(new { message = result, status = "OTP sent to email" });
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"Login failed for email {dto.Email}: {ex.Message}");
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
      try
      {
        _logger.LogInformation($"OTP verification attempt for email: {dto.Email}");
        var result = await _authService.VerifyOtp(dto);
        _logger.LogInformation($"OTP verified successfully for email: {dto.Email}");
        return Ok(new { success = true, message = "OTP verified successfully", data = result });
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"OTP verification failed for email {dto.Email}: {ex.Message}");
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromQuery] string token)
    {
      try
      {
        _logger.LogDebug("Token refresh attempt");
        var result = await _authService.RefreshToken(token);
        _logger.LogDebug("Token refreshed successfully");
        return Ok(result);
      }
      catch (Exception ex)
      {
        _logger.LogWarning($"Token refresh failed: {ex.Message}");
        return Unauthorized(new { message = ex.Message });
      }
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromQuery] string token)
    {
      try
      {
        _logger.LogInformation("Token revocation attempt");
        var result = await _authService.RevokeToken(token);
        _logger.LogInformation($"Token revoked successfully: {result}");
        return Ok(new { success = result });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Token revocation error: {ex.Message}");
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpGet("admin/users")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAllUsers()
    {
      try
      {
        _logger.LogInformation("Admin retrieving all users");
        var users = await _authService.GetAllUsersAsync();
        _logger.LogInformation($"Retrieved {users.Count} users");
        return Ok(new { success = true, message = "Users retrieved successfully", data = users });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving users: {ex.Message}");
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpPut("admin/users/{userId}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequestDto dto)
    {
      try
      {
        _logger.LogInformation($"Admin updating user role for userId: {userId} to role: {dto.Role}");
        var result = await _authService.UpdateUserRoleAsync(userId, dto);
        _logger.LogInformation($"User role updated successfully for userId: {userId}");
        return Ok(new { success = true, message = "User role updated successfully", data = result });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error updating user role for userId {userId}: {ex.Message}");
        return BadRequest(new { message = ex.Message });
      }
    }
  }
}

