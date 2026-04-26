using System;
using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);
        Task<AuthResponseDto> Login(LoginDto dto);
        Task<AuthResponseDto> VerifyOtp(VerifyOtpDto dto);
        Task<AuthResponseDto> RefreshToken(string token);
        Task<bool> RevokeToken(string token);
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto dto);
    }
}
