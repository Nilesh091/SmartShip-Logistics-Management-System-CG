using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services
{
  public class AuthService : IAuthService
  {
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService, IPasswordHasher hasher)
    {
      _userRepository = userRepository;
      _refreshTokenRepository = refreshTokenRepository;
      _tokenService = tokenService;
      _hasher = hasher;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
      var user = new User
      {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = _hasher.Hash(dto.Password),
        Role = "CUSTOMER"
      };

      await _userRepository.AddAsync(user);
      await _userRepository.SaveChangesAsync();

      return await GenerateTokens(user);
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
      var user = await _userRepository.GetByEmailAsync(dto.Email);

      if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
        throw new Exception("Invalid credentials");

      return await GenerateTokens(user);
    }

    public async Task<AuthResponseDto> RefreshToken(string token)
    {
      var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

      if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
        throw new Exception("Invalid refresh token");

      return await GenerateTokens(refreshToken.User);
    }

    public async Task<bool> RevokeToken(string token)
    {
      var rt = await _refreshTokenRepository.GetByTokenAsync(token);

      if (rt == null) return false;

      rt.IsRevoked = true;
      await _refreshTokenRepository.SaveChangesAsync();
      return true;
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
      var users = await _userRepository.GetAllAsync();
      return users.Select(u => new UserResponseDto
      {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role
      }).ToList();
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto dto)
    {
      var user = await _userRepository.GetByIdAsync(userId);

      if (user == null)
        throw new Exception("User not found");

      user.Role = dto.Role;
      await _userRepository.UpdateAsync(user);
      await _userRepository.SaveChangesAsync();

      return new UserResponseDto
      {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role
      };
    }

    private async Task<AuthResponseDto> GenerateTokens(User user)
    {
      var accessToken = _tokenService.GenerateAccessToken(user);
      var refreshToken = _tokenService.GenerateRefreshToken();

      var rt = new RefreshToken
      {
        Token = refreshToken,
        Expires = DateTime.UtcNow.AddDays(7),
        UserId = user.Id
      };

      await _refreshTokenRepository.AddAsync(rt);
      await _refreshTokenRepository.SaveChangesAsync();

      return new AuthResponseDto
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken
      };
    }
  }
}
