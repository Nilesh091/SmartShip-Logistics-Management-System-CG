using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using Shared.Events;
using Shared.Messaging;

namespace NotificationService.Infrastructure.Messaging
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EventDispatcher> _logger;

        public EventDispatcher(IEmailService emailService, ILogger<EventDispatcher> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Dispatch(string eventName, string message)
        {
            try
            {
                switch (eventName)
                {
                    case "otp-generated":
                        _logger.LogInformation($"Processing OTP generation event for message: {message}");
                        var otpEvent = JsonSerializer.Deserialize<OtpGeneratedEvent>(message);

                        if (otpEvent != null)
                        {
                            _logger.LogInformation($"Sending OTP email to: {otpEvent.Email}");
                            await _emailService.SendOtpEmailAsync(otpEvent.Email, otpEvent.Otp);
                            _logger.LogInformation($"OTP email sent successfully to: {otpEvent.Email}");
                        }
                        else
                        {
                            _logger.LogWarning("OtpGeneratedEvent deserialization failed");
                        }
                        break;

                    default:
                        _logger.LogWarning($"Unknown event type received: {eventName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing event '{eventName}': {ex.Message}");
                // Don't throw - log and continue to prevent service crash
            }
        }
    }
}
