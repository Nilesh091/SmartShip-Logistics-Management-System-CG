using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpUsername = _configuration["Smtp:Username"] ?? "your-email@gmail.com";
        var smtpPassword = _configuration["Smtp:Password"] ?? "your-app-password";
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@smartship.com";
        var fromName = _configuration["Smtp:FromName"] ?? "SmartShip";

        _logger.LogInformation($"Preparing to send OTP email to {toEmail}");
        _logger.LogInformation($"SMTP Configuration - Host: {smtpHost}, Port: {smtpPort}, Username: {smtpUsername.Substring(0, Math.Min(3, smtpUsername.Length))}***");

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(fromName, fromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = "Your OTP Code - SmartShip";

        email.Body = new TextPart("html")
        {
            Text = $@"
    <div style='font-family: Arial, sans-serif; background-color: #f4f6f7; padding: 20px;'>
        
        <div style='max-width: 500px; margin: auto; background: #ffffff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
            
            <h2 style='color: #2E86C1; text-align: center;'>🔐 SmartShip OTP Verification</h2>
            
            <p>Hello,</p>
            
            <p>Your One-Time Password (OTP) is:</p>
            
            <div style='
                font-size: 26px; 
                font-weight: bold; 
                text-align: center;
                background: #eef2f7; 
                padding: 12px; 
                border-radius: 6px; 
                letter-spacing: 4px;
                margin: 15px 0;
                color: #000;
            '>
                {otp}
            </div>
            
            <p style='text-align: center;'>
                ⏳ This code will expire in <strong>5 minutes</strong>.
            </p>
            
            <p style='margin-top: 20px;'>
                If you did not request this OTP, please ignore this email.
            </p>
            
            <hr style='margin: 20px 0;' />
            
            <p style='font-size: 12px; color: #777; text-align: center;'>
                This is an automated email. Please do not reply.
            </p>
            
            <p style='text-align: center; font-weight: bold;'>
                SmartShip Logistics Management System
            </p>
        
        </div>
    </div>
    "
        };
        using var smtp = new SmtpClient();

        try
        {
            _logger.LogInformation($"Connecting to SMTP server {smtpHost}:{smtpPort}");
            await smtp.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

            _logger.LogInformation($"Authenticating with username: {smtpUsername}");
            await smtp.AuthenticateAsync(smtpUsername, smtpPassword);

            _logger.LogInformation($"Sending email from {fromEmail} to {toEmail}");
            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
            _logger.LogInformation($"OTP email sent successfully to {toEmail}");
        }
        catch (MailKit.Security.AuthenticationException authEx)
        {
            _logger.LogError(authEx, $"SMTP Authentication failed for user {smtpUsername}. Ensure you're using an app-specific password for Gmail, not your regular password.");
            throw new InvalidOperationException($"SMTP authentication failed. For Gmail, use an App Password, not your regular password. Error: {authEx.Message}", authEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
            throw new InvalidOperationException($"Failed to send OTP email to {toEmail}: {ex.Message}", ex);
        }
    }
}
