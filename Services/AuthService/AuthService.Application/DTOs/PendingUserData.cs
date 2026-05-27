namespace AuthService.Application.DTOs
{
    public class PendingUserData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
