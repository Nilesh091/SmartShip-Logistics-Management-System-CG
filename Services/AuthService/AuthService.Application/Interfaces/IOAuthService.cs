using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IOAuthService
    {
        Task<AuthResponseDto> HandleGoogleCallbackAsync(string code);
    }
}
