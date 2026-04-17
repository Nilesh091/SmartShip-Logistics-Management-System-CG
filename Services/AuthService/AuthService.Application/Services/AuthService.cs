using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Shared.Messaging;
using Shared.Events;

namespace AuthService.Application.Services
{
  public class AuthService : IAuthService
  {
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOtpCodeRepository _otpRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _hasher;
    private readonly IRabbitMQPublisher _publisher;

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IOtpCodeRepository otpRepository, ITokenService tokenService, IPasswordHasher hasher, IRabbitMQPublisher publisher)
    {
      _userRepository = userRepository;
      _refreshTokenRepository = refreshTokenRepository;
      _otpRepository = otpRepository;
      _tokenService = tokenService;
      _hasher = hasher;
      _publisher = publisher;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
      // Check if user already exists
      var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
      if (existingUser != null)
        throw new Exception("User with this email already exists");

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

      // Generate and send OTP for email verification
      await GenerateAndSendOtpAsync(user);

      return new AuthResponseDto
      {
        AccessToken = "OTP sent to email. Please verify your email to complete registration.",
        RefreshToken = null
      };
    }

    public async Task<string> Login(LoginDto dto)
    {
      var user = await _userRepository.GetByEmailAsync(dto.Email);

      if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
        throw new Exception("Invalid credentials");
      // Generate and send OTP
      if (user.Role == "ADMIN")
      {
        var tokens = await GenerateTokens(user);
        return tokens.AccessToken; // or return full response if needed
      }

      await GenerateAndSendOtpAsync(user);

      return "OTP sent to email";
    }

    public async Task<AuthResponseDto> VerifyOtp(VerifyOtpDto dto)
    {
      var otp = await _otpRepository.GetByEmailAndCodeAsync(dto.Email, dto.Otp);

      if (otp == null || otp.ExpiryTime < DateTime.UtcNow)
        throw new Exception("Invalid or expired OTP");

      var user = await _userRepository.GetByEmailAsync(dto.Email);

      if (user == null)
        throw new Exception("User not found");

      // Delete OTP after successful verification
      await _otpRepository.DeleteAsync(otp);
      await _otpRepository.SaveChangesAsync();

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

    private async Task GenerateAndSendOtpAsync(User user)
    {
      // Generate 6-digit OTP
      var otp = new Random().Next(100000, 999999).ToString();

      // Delete old OTP for this email (prevent multiple OTPs)
      var oldOtp = await _otpRepository.GetByEmailAsync(user.Email);
      if (oldOtp != null)
      {
        await _otpRepository.DeleteAsync(oldOtp);
      }

      // Create OTP entity
      var otpEntity = new OtpCode
      {
        Id = Guid.NewGuid(),
        Email = user.Email,
        Code = otp,
        ExpiryTime = DateTime.UtcNow.AddMinutes(5)
      };

      await _otpRepository.AddAsync(otpEntity);
      await _otpRepository.SaveChangesAsync();

      // Publish event to RabbitMQ for email notification
      _publisher.Publish("otp-generated", new OtpGeneratedEvent
      {
        Email = user.Email,
        Otp = otp
      });
    }
  }
}
