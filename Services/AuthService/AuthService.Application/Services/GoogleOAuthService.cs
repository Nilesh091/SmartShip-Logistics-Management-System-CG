using System.Net.Http.Json;
using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleOAuthService> _logger;
        private readonly HttpClient _httpClient;

        public GoogleOAuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IConfiguration config,
            ILogger<GoogleOAuthService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<AuthResponseDto> HandleGoogleCallbackAsync(string code)
        {
            // Exchange code for tokens
            var tokenResponse = await ExchangeCodeAsync(code);
            var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);

            // Upsert user
            var user = await _userRepository.GetByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    PasswordHash = string.Empty,
                    Role = "CUSTOMER"
                };
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                _logger.LogInformation("New OAuth user created: {Email}", user.Email);
            }
            else
            {
                _logger.LogInformation("Existing OAuth user logged in: {Email}", user.Email);
            }

            // Generate JWT tokens
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
                Message = "OAuth login successful",
                Role = user.Role,
                RequiresOtp = false,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<GoogleTokenResponse> ExchangeCodeAsync(string code)
        {
            var clientId = _config["Google:ClientId"]!;
            var clientSecret = _config["Google:ClientSecret"]!;
            var redirectUri = _config["Google:RedirectUri"]!;

            var payload = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(payload));

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
                ?? throw new GoogleOAuthException("Failed to parse Google token response");
        }

        private async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://www.googleapis.com/oauth2/v2/userinfo");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GoogleUserInfo>()
                ?? throw new GoogleOAuthException("Failed to parse Google user info");
        }

        private record GoogleTokenResponse(
            [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string AccessToken,
            [property: System.Text.Json.Serialization.JsonPropertyName("id_token")] string IdToken);

        private record GoogleUserInfo(
            [property: System.Text.Json.Serialization.JsonPropertyName("email")] string Email,
            [property: System.Text.Json.Serialization.JsonPropertyName("name")] string Name);
    }
}
