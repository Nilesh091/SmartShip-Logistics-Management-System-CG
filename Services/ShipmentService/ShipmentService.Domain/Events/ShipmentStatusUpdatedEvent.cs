using System;

namespace ShipmentService.Domain.Events
{
    public class ShipmentStatusUpdatedEvent
    {
        public Guid ShipmentId { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
