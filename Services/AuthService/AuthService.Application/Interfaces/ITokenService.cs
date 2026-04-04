using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
  public interface ITokenService
  {
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
  }
}
