using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Shared.Messaging;
using Shared.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

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
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;
    private const string PENDING_USER_CACHE_KEY = "pending_user_{0}";

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IOtpCodeRepository otpRepository, ITokenService tokenService, IPasswordHasher hasher, IRabbitMQPublisher publisher, IMemoryCache cache, ILogger<AuthService> logger)
    {
      _userRepository = userRepository;
      _refreshTokenRepository = refreshTokenRepository;
      _otpRepository = otpRepository;
      _tokenService = tokenService;
      _hasher = hasher;
      _publisher = publisher;
      _cache = cache;
      _logger = logger;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
      try
      {
        _logger.LogInformation($"Starting user registration for email: {dto.Email}");

        // Check if user already exists in Users table
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
          _logger.LogWarning($"Registration attempt with existing email: {dto.Email}");
          throw new Exception("User with this email already exists");
        }

        // Step 1: Store temporary user data in CACHE (not in database)
        var otp = new Random().Next(100000, 999999).ToString();

        var pendingUserData = new
        {
          Name = dto.Name,
          Email = dto.Email,
          PasswordHash = _hasher.Hash(dto.Password),
          Otp = otp,
          CreatedAt = DateTime.UtcNow
        };

        // Store in cache with 5 minute expiration
        var cacheKey = string.Format(PENDING_USER_CACHE_KEY, dto.Email);
        var cacheOptions = new MemoryCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, pendingUserData, cacheOptions);
        _logger.LogInformation($"Pending user data stored in cache (expires in 5 min) for email: {dto.Email}");

        // Publish event to send OTP email
        _publisher.Publish("otp-generated", new OtpGeneratedEvent
        {
          Email = dto.Email,
          Otp = otp
        });
        _logger.LogInformation($"OTP generation event published for email: {dto.Email}");

        _logger.LogInformation($"Registration flow started for: {dto.Email}");
        return new AuthResponseDto
        {
          AccessToken = "OTP sent to email. Please verify your email to complete registration.",
          RefreshToken = null
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Registration failed for email {dto.Email}: {ex.Message}");
        throw;
      }
    }

    public async Task<string> Login(LoginDto dto)
    {
      try
      {
        _logger.LogInformation($"Login attempt for email: {dto.Email}");
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || !_hasher.Verify(dto.Password, user.PasswordHash))
        {
          _logger.LogWarning($"Invalid credentials for email: {dto.Email}");
          throw new Exception("Invalid credentials");
        }

        _logger.LogInformation($"Credentials verified for user: {dto.Email}");

        // Generate and send OTP
        if (user.Role == "ADMIN")
        {
          _logger.LogInformation($"Admin login detected for: {dto.Email}");
          var tokens = await GenerateTokens(user);
          return tokens.AccessToken; // or return full response if needed
        }

        await GenerateAndSendOtpAsync(user);
        _logger.LogInformation($"OTP sent to email for user: {dto.Email}");

        return "OTP sent to email";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Login failed for email {dto.Email}: {ex.Message}");
        throw;
      }
    }

    public async Task<AuthResponseDto> VerifyOtp(VerifyOtpDto dto)
    {
      try
      {
        _logger.LogInformation($"OTP verification attempt for email: {dto.Email}");

        // Step 1: Retrieve pending user data from CACHE
        var cacheKey = string.Format(PENDING_USER_CACHE_KEY, dto.Email);
        if (!_cache.TryGetValue(cacheKey, out dynamic pendingUserData))
        {
          _logger.LogWarning($"No pending registration found in cache for email: {dto.Email}");
          throw new Exception("No pending registration found. Please register first.");
        }

        // Step 2: Verify OTP
        if (pendingUserData.Otp != dto.Otp)
        {
          _logger.LogWarning($"Invalid OTP for email: {dto.Email}");
          throw new Exception("Invalid OTP");
        }

        _logger.LogInformation($"OTP verified successfully for email: {dto.Email}");

        // Step 3: Create the actual user in database after OTP verification
        var user = new User
        {
          Id = Guid.NewGuid(),
          Name = pendingUserData.Name,
          Email = pendingUserData.Email,
          PasswordHash = pendingUserData.PasswordHash,
          Role = "CUSTOMER"
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();
        _logger.LogInformation($"User successfully created after OTP verification with ID: {user.Id}");

        // Step 4: Delete cache entry
        _cache.Remove(cacheKey);
        _logger.LogInformation($"Pending user data removed from cache for email: {dto.Email}");

        // Step 5: Generate tokens
        var tokens = await GenerateTokens(user);
        _logger.LogInformation($"Tokens generated for newly registered user: {dto.Email}");

        return tokens;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"OTP verification failed for email {dto.Email}: {ex.Message}");
        throw;
      }
    }

    public async Task<AuthResponseDto> RefreshToken(string token)
    {
      try
      {
        _logger.LogDebug("Token refresh attempt");
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

        if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
        {
          _logger.LogWarning("Invalid or expired refresh token");
          throw new Exception("Invalid refresh token");
        }

        _logger.LogDebug("Refresh token validated successfully");
        return await GenerateTokens(refreshToken.User);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Token refresh failed: {ex.Message}");
        throw;
      }
    }

    public async Task<bool> RevokeToken(string token)
    {
      try
      {
        _logger.LogInformation("Token revocation attempt");
        var rt = await _refreshTokenRepository.GetByTokenAsync(token);

        if (rt == null)
        {
          _logger.LogWarning("Token not found for revocation");
          return false;
        }

        rt.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync();
        _logger.LogInformation("Token revoked successfully");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Token revocation failed: {ex.Message}");
        throw;
      }
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
      try
      {
        _logger.LogInformation("Retrieving all users");
        var users = await _userRepository.GetAllAsync();
        _logger.LogInformation($"Retrieved {users.Count} users from database");
        return users.Select(u => new UserResponseDto
        {
          Id = u.Id,
          Name = u.Name,
          Email = u.Email,
          Role = u.Role
        }).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving users: {ex.Message}");
        throw;
      }
    }

    public async Task<UserResponseDto> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequestDto dto)
    {
      try
      {
        _logger.LogInformation($"Updating user role for userId: {userId} to role: {dto.Role}");
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
          _logger.LogWarning($"User not found with ID: {userId}");
          throw new Exception("User not found");
        }

        user.Role = dto.Role;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        _logger.LogInformation($"User role updated successfully for userId: {userId} to {dto.Role}");

        return new UserResponseDto
        {
          Id = user.Id,
          Name = user.Name,
          Email = user.Email,
          Role = user.Role
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error updating user role for userId {userId}: {ex.Message}");
        throw;
      }
    }

    private async Task<AuthResponseDto> GenerateTokens(User user)
    {
      try
      {
        _logger.LogDebug($"Generating tokens for user: {user.Email}");
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
        _logger.LogDebug($"Tokens generated and saved for user: {user.Email}");

        return new AuthResponseDto
        {
          AccessToken = accessToken,
          RefreshToken = refreshToken
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error generating tokens for user {user.Email}: {ex.Message}");
        throw;
      }
    }

    private async Task GenerateAndSendOtpAsync(User user)
    {
      try
      {
        _logger.LogInformation($"Generating OTP for user: {user.Email}");
        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();

        // Delete old OTP for this email (prevent multiple OTPs)
        var oldOtp = await _otpRepository.GetByEmailAsync(user.Email);
        if (oldOtp != null)
        {
          await _otpRepository.DeleteAsync(oldOtp);
          _logger.LogDebug($"Deleted old OTP for user: {user.Email}");
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
        _logger.LogDebug($"OTP saved to database for user: {user.Email}");

        // Publish event to RabbitMQ for email notification
        _publisher.Publish("otp-generated", new OtpGeneratedEvent
        {
          Email = user.Email,
          Otp = otp
        });
        _logger.LogInformation($"OTP generation event published for user: {user.Email}");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error generating and sending OTP for user {user.Email}: {ex.Message}");
        throw;
      }
    }
  }
}
