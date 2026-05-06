using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly IConfiguration _config;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(IOAuthService oauthService, IConfiguration config, ILogger<OAuthController> logger)
        {
            _oauthService = oauthService;
            _config = config;
            _logger = logger;
        }

        /// <summary>Redirects the browser to Google's OAuth consent screen.</summary>
        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            var clientId = _config["Google:ClientId"]!;
            var redirectUri = Uri.EscapeDataString(_config["Google:RedirectUri"]!);
            var scope = Uri.EscapeDataString("openid email profile");
            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={clientId}" +
                      $"&redirect_uri={redirectUri}" +
                      $"&response_type=code" +
                      $"&scope={scope}" +
                      $"&access_type=offline";
            return Redirect(url);
        }

        /// <summary>Google redirects here with the auth code; we exchange it for tokens and redirect to frontend.</summary>
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string? error)
        {
            var frontendBase = _config["Frontend:BaseUrl"]!;

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("Google OAuth error: {Error}", error);
                return Redirect($"{frontendBase}/login?oauth_error={Uri.EscapeDataString(error)}");
            }

            try
            {
                var result = await _oauthService.HandleGoogleCallbackAsync(code);
                var redirectUrl = $"{frontendBase}/oauth/callback" +
                                  $"?accessToken={Uri.EscapeDataString(result.AccessToken!)}" +
                                  $"&refreshToken={Uri.EscapeDataString(result.RefreshToken!)}" +
                                  $"&role={Uri.EscapeDataString(result.Role!)}";
                return Redirect(redirectUrl);
            }
            catch (GoogleOAuthException ex)
            {
                _logger.LogWarning(ex, "Google OAuth callback failed with a custom OAuth exception");
                return Redirect($"{frontendBase}/login?oauth_error={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google OAuth callback failed");
                return Redirect($"{frontendBase}/login?oauth_error={Uri.EscapeDataString("Authentication failed")}");
            }
        }
    }
}
