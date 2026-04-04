using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Security
{
  public class TokenService : ITokenService
  {
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
      _config = config;
    }

    public string GenerateAccessToken(User user)
    {
      var claims = new[]
      {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

      var key = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(_config["Jwt:Key"])
      );

      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          issuer: _config["Jwt:Issuer"],
          audience: _config["Jwt:Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(15),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
      return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
  }
}
