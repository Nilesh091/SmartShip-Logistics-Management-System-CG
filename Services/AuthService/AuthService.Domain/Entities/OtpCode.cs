using System;

namespace AuthService.Domain.Entities
{
    public class OtpCode
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
