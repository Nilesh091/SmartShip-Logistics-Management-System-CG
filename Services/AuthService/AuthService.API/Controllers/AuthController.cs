using Microsoft.AspNetCore.Mvc;
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
        return Ok(result);
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
        return Ok(result);
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
  }
}
