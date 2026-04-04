using System;

namespace AuthService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Role { get; set; } // CUSTOMER / ADMIN

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
