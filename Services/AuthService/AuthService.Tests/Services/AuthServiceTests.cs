using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Messaging;

namespace AuthService.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepo;
    private Mock<IRefreshTokenRepository> _refreshTokenRepo;
    private Mock<IOtpCodeRepository> _otpRepo;
    private Mock<ITokenService> _tokenService;
    private Mock<IPasswordHasher> _hasher;
    private Mock<IRabbitMQPublisher> _publisher;
    private Mock<ILogger<AuthService.Application.Services.AuthService>> _logger;
    private IMemoryCache _cache;
    private AuthService.Application.Services.AuthService _service;

    [SetUp]
    public void Setup()
    {
        _userRepo = new Mock<IUserRepository>();
        _refreshTokenRepo = new Mock<IRefreshTokenRepository>();
        _otpRepo = new Mock<IOtpCodeRepository>();
        _tokenService = new Mock<ITokenService>();
        _hasher = new Mock<IPasswordHasher>();
        _publisher = new Mock<IRabbitMQPublisher>();
        _logger = new Mock<ILogger<AuthService.Application.Services.AuthService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _service = new AuthService.Application.Services.AuthService(
            _userRepo.Object,
            _refreshTokenRepo.Object,
            _otpRepo.Object,
            _tokenService.Object,
            _hasher.Object,
            _publisher.Object,
            _cache,
            _logger.Object
        );
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    // ── Register ──────────────────────────────────────────────────────────────

    [Test]
    public async Task Register_NewUser_ShouldReturnRequiresOtpTrueAndPublishEvent()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("new@example.com")).ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("pass123")).Returns("hashed");

        var result = await _service.Register(new RegisterDto
        {
            Name = "Test User",
            Email = "new@example.com",
            Password = "pass123"
        });

        Assert.That(result.RequiresOtp, Is.True);
        Assert.That(result.AccessToken, Is.Null);
        _publisher.Verify(p => p.Publish("otp-generated", It.IsAny<object>()), Times.Once);
    }

    [Test]
    public void Register_ExistingEmail_ShouldThrowAuthServiceException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("existing@example.com"))
                 .ReturnsAsync(new User { Email = "existing@example.com" });

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.Register(new RegisterDto
            {
                Name = "User",
                Email = "existing@example.com",
                Password = "pass"
            }));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task Login_ValidCustomer_ShouldReturnRequiresOtpTrue()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Role = "CUSTOMER", PasswordHash = "hashed" };
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("pass", "hashed")).Returns(true);
        _otpRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync((OtpCode?)null);

        var result = await _service.Login(new LoginDto { Email = "user@example.com", Password = "pass" });

        Assert.That(result.RequiresOtp, Is.True);
        Assert.That(result.AccessToken, Is.Null);
    }

    [Test]
    public async Task Login_ValidAdmin_ShouldReturnTokensDirectlyWithoutOtp()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "admin@example.com", Role = "ADMIN", PasswordHash = "hashed" };
        _userRepo.Setup(r => r.GetByEmailAsync("admin@example.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("adminpass", "hashed")).Returns(true);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

        var result = await _service.Login(new LoginDto { Email = "admin@example.com", Password = "adminpass" });

        Assert.That(result.RequiresOtp, Is.False);
        Assert.That(result.AccessToken, Is.EqualTo("access-token"));
        Assert.That(result.Role, Is.EqualTo("ADMIN"));
    }

    [Test]
    public void Login_InvalidPassword_ShouldThrowAuthServiceException()
    {
        var user = new User { Email = "user@example.com", PasswordHash = "hashed" };
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.Login(new LoginDto { Email = "user@example.com", Password = "wrong" }));
    }

    [Test]
    public void Login_UserNotFound_ShouldThrowAuthServiceException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("ghost@example.com")).ReturnsAsync((User?)null);

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.Login(new LoginDto { Email = "ghost@example.com", Password = "pass" }));
    }

    // ── VerifyOtp ─────────────────────────────────────────────────────────────

    [Test]
    public async Task VerifyOtp_RegistrationFlow_ValidOtp_ShouldCreateUserAndReturnTokens()
    {
        _cache.Set("pending_user_new@example.com", new PendingUserData
        {
            Name = "New User",
            Email = "new@example.com",
            PasswordHash = "hashed",
            Otp = "123456",
            CreatedAt = DateTime.UtcNow
        });

        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var result = await _service.VerifyOtp(new VerifyOtpDto { Email = "new@example.com", Otp = "123456" });

        Assert.That(result.RequiresOtp, Is.False);
        Assert.That(result.AccessToken, Is.EqualTo("access"));
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void VerifyOtp_RegistrationFlow_WrongOtp_ShouldThrowAuthServiceException()
    {
        _cache.Set("pending_user_new@example.com", new PendingUserData
        {
            Name = "New User",
            Email = "new@example.com",
            PasswordHash = "hashed",
            Otp = "123456",
            CreatedAt = DateTime.UtcNow
        });

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.VerifyOtp(new VerifyOtpDto { Email = "new@example.com", Otp = "000000" }));
    }

    [Test]
    public async Task VerifyOtp_LoginFlow_ValidOtp_ShouldReturnTokensAndDeleteOtp()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "login@example.com", Role = "CUSTOMER" };
        var otpRecord = new OtpCode
        {
            Email = "login@example.com",
            Code = "654321",
            ExpiryTime = DateTime.UtcNow.AddMinutes(5)
        };

        _otpRepo.Setup(r => r.GetByEmailAsync("login@example.com")).ReturnsAsync(otpRecord);
        _userRepo.Setup(r => r.GetByEmailAsync("login@example.com")).ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh");

        var result = await _service.VerifyOtp(new VerifyOtpDto { Email = "login@example.com", Otp = "654321" });

        Assert.That(result.RequiresOtp, Is.False);
        Assert.That(result.AccessToken, Is.EqualTo("access"));
        _otpRepo.Verify(r => r.DeleteAsync(otpRecord), Times.Once);
        _otpRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void VerifyOtp_LoginFlow_ExpiredOtp_ShouldThrowAuthServiceException()
    {
        var otpRecord = new OtpCode
        {
            Email = "login@example.com",
            Code = "654321",
            ExpiryTime = DateTime.UtcNow.AddMinutes(-1)
        };
        _otpRepo.Setup(r => r.GetByEmailAsync("login@example.com")).ReturnsAsync(otpRecord);

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.VerifyOtp(new VerifyOtpDto { Email = "login@example.com", Otp = "654321" }));
    }

    [Test]
    public void VerifyOtp_LoginFlow_WrongOtp_ShouldThrowAuthServiceException()
    {
        var otpRecord = new OtpCode
        {
            Email = "login@example.com",
            Code = "654321",
            ExpiryTime = DateTime.UtcNow.AddMinutes(5)
        };
        _otpRepo.Setup(r => r.GetByEmailAsync("login@example.com")).ReturnsAsync(otpRecord);

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.VerifyOtp(new VerifyOtpDto { Email = "login@example.com", Otp = "000000" }));
    }

    [Test]
    public void VerifyOtp_LoginFlow_NoOtpRecord_ShouldThrowAuthServiceException()
    {
        _otpRepo.Setup(r => r.GetByEmailAsync("login@example.com")).ReturnsAsync((OtpCode?)null);

        Assert.ThrowsAsync<AuthServiceException>(() =>
            _service.VerifyOtp(new VerifyOtpDto { Email = "login@example.com", Otp = "123456" }));
    }

    // ── RefreshToken ──────────────────────────────────────────────────────────

    [Test]
    public async Task RefreshToken_ValidToken_ShouldReturnNewTokens()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com" };
        var rt = new RefreshToken { Token = "valid-rt", Expires = DateTime.UtcNow.AddDays(1), User = user };

        _refreshTokenRepo.Setup(r => r.GetByTokenAsync("valid-rt")).ReturnsAsync(rt);
        _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("new-access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");

        var result = await _service.RefreshToken("valid-rt");

        Assert.That(result.AccessToken, Is.EqualTo("new-access"));
        Assert.That(result.RefreshToken, Is.EqualTo("new-refresh"));
    }

    [Test]
    public void RefreshToken_ExpiredToken_ShouldThrowAuthServiceException()
    {
        var rt = new RefreshToken { Token = "expired-rt", Expires = DateTime.UtcNow.AddDays(-1), User = new User() };
        _refreshTokenRepo.Setup(r => r.GetByTokenAsync("expired-rt")).ReturnsAsync(rt);

        Assert.ThrowsAsync<AuthServiceException>(() => _service.RefreshToken("expired-rt"));
    }

    [Test]
    public void RefreshToken_TokenNotFound_ShouldThrowAuthServiceException()
    {
        _refreshTokenRepo.Setup(r => r.GetByTokenAsync("ghost")).ReturnsAsync((RefreshToken?)null);

        Assert.ThrowsAsync<AuthServiceException>(() => _service.RefreshToken("ghost"));
    }

    // ── RevokeToken ───────────────────────────────────────────────────────────

    [Test]
    public async Task RevokeToken_ExistingToken_ShouldMarkRevokedAndReturnTrue()
    {
        var rt = new RefreshToken { Token = "rt", IsRevoked = false };
        _refreshTokenRepo.Setup(r => r.GetByTokenAsync("rt")).ReturnsAsync(rt);

        var result = await _service.RevokeToken("rt");

        Assert.That(result, Is.True);
        Assert.That(rt.IsRevoked, Is.True);
        _refreshTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task RevokeToken_TokenNotFound_ShouldReturnFalse()
    {
        _refreshTokenRepo.Setup(r => r.GetByTokenAsync("missing")).ReturnsAsync((RefreshToken?)null);

        var result = await _service.RevokeToken("missing");

        Assert.That(result, Is.False);
        _refreshTokenRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
