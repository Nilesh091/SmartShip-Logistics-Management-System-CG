using System;

namespace Shared.Events
{
    public class OtpGeneratedEvent
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
