using System;

namespace AuthService.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Message { get; set; }
        public string Role { get; set; }
        public bool RequiresOtp { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
