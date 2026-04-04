using AuthService.Application.Interfaces;
using BCrypt.Net;

namespace AuthService.Infrastructure.Security
{
  public class PasswordHasher : IPasswordHasher
  {
    public string Hash(string password)
    {
      return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
      return BCrypt.Net.BCrypt.Verify(password, hash);
    }
  }
}
